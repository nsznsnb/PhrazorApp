using Microsoft.EntityFrameworkCore;
using PhrazorApp.Common; // ServiceResult / Unit
using PhrazorApp.Data.Entities;
using PhrazorApp.Data.UnitOfWork;
using PhrazorApp.Models;
using PhrazorApp.Models.Mappings;

namespace PhrazorApp.Services
{
    public sealed class GradeService
    {
        private readonly UnitOfWork _uow;
        private const string MSG_PREFIX = "成績";
        public GradeService(UnitOfWork uow) => _uow = uow;

        // 既定判定ルール（S/A/B/D）
        private static readonly (string Name, int MinPct)[] DefaultRules =
            { ("S", 90), ("A", 75), ("B", 60), ("D", 0) };

        // 並び順で取得
        public Task<ServiceResult<List<GradeModel>>> GetListAsync()
            => _uow.ReadAsync(async repos =>
            {
                var list = await repos.Grades.Queryable(/*asNoTracking=*/true)
                    .OrderBy(x => x.OrderNo).ThenBy(x => x.GradeName)
                    .SelectModel()
                    .ToListAsync();
                return ServiceResult.Success(list, "");
            });

        public Task<ServiceResult<GradeModel?>> GetAsync(Guid id)
            => _uow.ReadAsync(async repos =>
            {
                var e = await repos.Grades.GetByIdAsync(id);
                return ServiceResult.Success(e?.ToModel(), "");
            });

        /// <summary>成績名で取得（見つからなければ null）</summary>
        public Task<MGrade?> GetByNameAsync(string name)
            => _uow.ReadAsync(async r =>
                await r.Grades.Queryable(true).FirstOrDefaultAsync(x => x.GradeName == name));

        /// <summary>正答率→成績名（"S"/"A"/"B"/"D"）</summary>
        private static string SymbolFromRate(double rate)
        {
            var pct = (int)Math.Round(rate * 100.0, MidpointRounding.AwayFromZero);
            return DefaultRules.First(x => pct >= x.MinPct).Name;
        }

        /// ★ 正答率から必ず成績を返す（未登録なら作成）
        public async Task<MGrade> ResolveByRateEnsureAsync(double rate)
        {
            var symbol = SymbolFromRate(rate);

            // 既存を探す（NoTracking）
            var found = await _uow.ReadAsync(async r =>
                await r.Grades.Queryable(true).FirstOrDefaultAsync(x => x.GradeName == symbol));
            if (found is not null) return found;

            // 無ければ末尾 OrderNo で新規作成
            var now = DateTime.UtcNow;
            var max = await _uow.ReadAsync(async r =>
                await r.Grades.Queryable(true).MaxAsync(x => (int?)x.OrderNo) ?? -1);

            var e = new MGrade
            {
                GradeId = Guid.NewGuid(),
                GradeName = symbol,
                OrderNo = max + 1,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _uow.ExecuteInTransactionAsync(async repos => { await repos.Grades.AddAsync(e); });
            return e;
        }

        // --- CRUD（既存のまま。Queryable(true) を使うよう微修正） ---

        public async Task<ServiceResult<Unit>> CreateAsync(GradeModel model)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async repos =>
                {
                    if (model.Id == Guid.Empty) model.Id = Guid.NewGuid();

                    var exists = await repos.Grades.Queryable(true)
                        .AnyAsync(x => x.GradeName == model.Name);
                    if (exists) throw new InvalidOperationException("同名の成績が既に存在します。");

                    var max = await repos.Grades.Queryable(true)
                        .MaxAsync(x => (int?)x.OrderNo) ?? -1;

                    var e = model.ToEntity();
                    e.OrderNo = max + 1; // 末尾
                    await repos.Grades.AddAsync(e);
                });
                return ServiceResult.None.Success(string.Format(AppMessages.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX));
            }
            catch
            {
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_CREATE_DETAIL, MSG_PREFIX));
            }
        }

        public async Task<ServiceResult<Unit>> UpdateAsync(GradeModel model)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async repos =>
                {
                    var e = await repos.Grades.GetByIdAsync(model.Id)
                        ?? throw new InvalidOperationException("対象が見つかりません。");

                    var exists = await repos.Grades.Queryable(true)
                        .AnyAsync(x => x.GradeName == model.Name && x.GradeId != model.Id);
                    if (exists) throw new InvalidOperationException("同名の成績が既に存在します。");

                    model.ApplyTo(e); // OrderNo 含む
                    await repos.Grades.UpdateAsync(e);
                });
                return ServiceResult.None.Success(string.Format(AppMessages.MSG_I_SUCCESS_UPDATE_DETAIL, MSG_PREFIX));
            }
            catch
            {
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_UPDATE_DETAIL, MSG_PREFIX));
            }
        }

        public async Task<ServiceResult<Unit>> DeleteAsync(Guid id)
        {
            try
            {
                var hasRef = await _uow.ReadAsync(async r =>
                    await r.TestResults.Queryable(true).AnyAsync(x => x.GradeId == id));
                if (hasRef)
                    return ServiceResult.None.Warning("この成績はテスト結果で使用されています。削除できません。");

                await _uow.ExecuteInTransactionAsync(async r =>
                {
                    var e = await r.Grades.GetByIdAsync(id)
                        ?? throw new InvalidOperationException("対象が見つかりません。");
                    await r.Grades.DeleteAsync(e);
                });
                return ServiceResult.None.Success(string.Format(AppMessages.MSG_I_SUCCESS_DELETE_DETAIL, MSG_PREFIX));
            }
            catch
            {
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_DELETE_DETAIL, MSG_PREFIX));
            }
        }

        public async Task<ServiceResult<Unit>> SaveOrderAsync(IReadOnlyList<GradeModel> models)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async repos =>
                {
                    var ids = models.Select(m => m.Id).ToList();
                    var indexMap = ids.Select((id, i) => new { id, i })
                                      .ToDictionary(x => x.id, x => x.i);

                    var entities = await repos.Grades.Queryable()
                        .Where(x => ids.Contains(x.GradeId))
                        .ToListAsync();

                    foreach (var e in entities)
                    {
                        e.OrderNo = indexMap[e.GradeId];
                        await repos.Grades.UpdateAsync(e);
                    }
                });
                return ServiceResult.None.Success("並び順を保存しました。");
            }
            catch
            {
                return ServiceResult.None.Error("並び順の保存に失敗しました。");
            }
        }
    }
}
