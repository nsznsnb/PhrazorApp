using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data.Entities;
using PhrazorApp.Data.UnitOfWork;
using PhrazorApp.Models;
using PhrazorApp.Models.Dtos;
using PhrazorApp.Models.Mappings;

namespace PhrazorApp.Services
{
    public sealed class ProverbService
    {
        private readonly UnitOfWork _uow;
        private readonly ILogger<ProverbService> _log;
        private const string MSG_PREFIX = "格言";

        public ProverbService(UnitOfWork uow, ILogger<ProverbService> log)
        {
            _uow = uow;
            _log = log;
        }

        public Task<ServiceResult<List<ProverbModel>>> GetListAsync()
        {
            return _uow.ReadAsync(async (UowRepos repos) =>
            {
                var list = await repos.Proverbs.GetListProjectedAsync();
                return ServiceResult.Success(list, "");
            });
        }

        public Task<ServiceResult<ProverbModel>> GetAsync(Guid id)
        {
            return _uow.ReadAsync(async (UowRepos repos) =>
            {
                var e = await repos.Proverbs.GetByIdAsync(id);
                if (e is null) return ServiceResult.Error<ProverbModel>($"{MSG_PREFIX}が見つかりません。");
                return ServiceResult.Success(e.ToModel(), "");
            });
        }

        public async Task<ServiceResult<Unit>> CreateAsync(ProverbModel model)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async (UowRepos repos) =>
                {
                    if (model.Id == Guid.Empty) model.Id = Guid.NewGuid();
                    await repos.Proverbs.AddAsync(model.ToEntityForCreate());
                });

                return ServiceResult.None.Success(string.Format(AppMessages.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX));
            }
            catch
            {
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_CREATE_DETAIL, MSG_PREFIX));
            }
        }

        public async Task<ServiceResult<Unit>> UpdateAsync(ProverbModel model)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async (UowRepos repos) =>
                {
                    var e = await repos.Proverbs.GetByIdAsync(model.Id)
                        ?? throw new InvalidOperationException("対象が見つかりません。");

                    model.ApplyTo(e);
                    await repos.Proverbs.UpdateAsync(e);
                });

                return ServiceResult.None.Success(string.Format(AppMessages.MSG_I_SUCCESS_UPDATE_DETAIL, MSG_PREFIX));
            }
            catch
            {
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_UPDATE_DETAIL, MSG_PREFIX));
            }
        }



        public async Task<ServiceResult<Unit>> UpsertAsync(ProverbModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Text))
                return ServiceResult.None.Success("格言は必須です。");

            // 正規化（null/trimをここで揃える）
            var text = model.Text.Trim();
            var author = string.IsNullOrWhiteSpace(model.Author) ? null : model.Author!.Trim();
            var meaning = string.IsNullOrWhiteSpace(model.Meaning) ? null : model.Meaning!.Trim();

            try
            {
                await _uow.ExecuteInTransactionAsync(async (UowRepos repos) =>
                {
                    var ent = await repos.Proverbs.GetByTextAuthorAsync(text, author);

                    if (ent is null)
                    {
                        // 新規
                        await repos.Proverbs.AddAsync(new MProverb
                        {
                            ProverbId = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id,
                            ProverbText = text,
                            Author = author,
                            Meaning = meaning
                        });
                    }
                    else
                    {
                        // 更新
                        ent.ProverbText = text;   // 仕様上キー同等だが念のため整合
                        ent.Author = author;
                        ent.Meaning = meaning;
                        await repos.Proverbs.UpdateAsync(ent);
                    }
                });

                return ServiceResult.None.Success($"{MSG_PREFIX}を保存しました。");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Proverb upsert error");
                return ServiceResult.None.Success($"{MSG_PREFIX}の保存に失敗しました。");
            }
        }

        public async Task<ServiceResult<Unit>> DeleteAsync(Guid id)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async (UowRepos repos) =>
                {
                    var e = await repos.Proverbs.GetByIdAsync(id) ?? throw new InvalidOperationException("not found");
                    await repos.Proverbs.DeleteAsync(e);
                });
                return ServiceResult.None.Success($"{MSG_PREFIX}を削除しました。");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Proverb delete error");
                return ServiceResult.None.Error($"{MSG_PREFIX}の削除に失敗しました。");
            }
        }

        // ProverbService に追記（Unit を返す版：UiRunner.WriteThenReloadAsyncと整合）
        public async Task<ServiceResult<Unit>> DeleteProverbsAsync(IReadOnlyCollection<Guid> ids)
        {
            if (ids is null || ids.Count == 0)
                return ServiceResult.None.Success("削除対象がありません。");

            try
            {
                var deleted = 0;

                await _uow.ExecuteInTransactionAsync(async (UowRepos repos) =>
                {
                    // BaseRepository.Queryable() を使用して対象を収集
                    var targets = await repos.Proverbs
                                             .Queryable() // 追跡あり（削除に適す）
                                             .Where(p => ids.Contains(p.ProverbId))
                                             .ToListAsync();

                    if (targets.Count == 0) return;

                    // BaseRepository.DeleteRangeAsync() で一括削除（SaveChanges は UoW 側）
                    await repos.Proverbs.DeleteRangeAsync(targets);
                    deleted = targets.Count;
                });

                var msg = deleted == 0
                    ? $"{MSG_PREFIX}は削除されませんでした。"
                    : $"{MSG_PREFIX}を {deleted} 件削除しました。";

                return ServiceResult.None.Success(msg);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Proverb bulk delete error");
                return ServiceResult.None.Error($"{MSG_PREFIX}の一括削除に失敗しました。");
            }
        }


        /// <summary>CSV 行の Upsert（Text+Author をキー）</summary>
        public async Task<ServiceResult<Unit>> ImportCsvAsync(IEnumerable<ProverbImportDto> rows)
        {
            if (rows is null) return ServiceResult.None.Error("CSV にデータがありません。");

            try
            {
                await _uow.ExecuteInTransactionAsync(async (UowRepos repos) =>
                {
                    var ents = rows
                        .Where(r => !string.IsNullOrWhiteSpace(r.Text))
                        .Select(r => new MProverb
                        {
                            ProverbId = Guid.NewGuid(),
                            ProverbText = r.Text!.Trim(),
                            Meaning = string.IsNullOrWhiteSpace(r.Meaning) ? null : r.Meaning!.Trim(),
                            Author = string.IsNullOrWhiteSpace(r.Author) ? null : r.Author!.Trim()
                        })
                        .ToList();

                    if (ents.Count == 0) return;

                    await repos.Proverbs.UpsertRangeByTextAuthorAsync(ents);
                });

                return ServiceResult.None.Success("CSV を取り込みました。");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Proverb CSV import error");
                return ServiceResult.None.Success("CSV 取込に失敗しました。");
            }
        }
    }
}
