using Microsoft.EntityFrameworkCore;
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

        // 既定の判定ルール（S/A/B/D）
        private static readonly (string Name, int MinPct)[] DefaultRules =
        {
            ("S", 90), ("A", 75), ("B", 60), ("D", 0)
        };

        // 変更：並び順で取得
        public Task<ServiceResult<List<GradeModel>>> GetListAsync()
        {
            return _uow.ReadAsync(async repos =>
            {
                var list = await repos.Grades.Queryable()
                    .OrderBy(x => x.OrderNo).ThenBy(x => x.GradeName)
                    .SelectModel()
                    .ToListAsync();

                return ServiceResult.Success(list, message: "");
            });
        }

        public Task<ServiceResult<GradeModel?>> GetAsync(Guid id)
        {
            return _uow.ReadAsync(async repos =>
            {
                var e = await repos.Grades.GetByIdAsync(id);
                return ServiceResult.Success(e?.ToModel(), message: "");
            });
        }

        /// <summary>テーブルが空なら S/A/B/D を作成</summary>
        public async Task EnsureDefaultsAsync()
        {
            await _uow.ExecuteInTransactionAsync(async repos =>
            {
                var any = await repos.Grades.Queryable().AnyAsync(); // 既存APIのみ
                if (any) return;

                var now = DateTime.UtcNow;
                int order = 1;
                foreach (var (name, _) in DefaultRules)
                {
                    var e = new MGrade
                    {
                        GradeId = Guid.NewGuid(),
                        GradeName = name,
                        OrderNo = order++,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    await repos.Grades.AddAsync(e);
                }
            });
        }

        /// <summary>成績名（"S"/"A"/"B"/"D"）で取得</summary>
        public async Task<MGrade?> GetByNameAsync(string name)
        {
            return await _uow.ReadAsync(async u =>
            {
                return await u.Grades.Queryable()
                    .FirstOrDefaultAsync(x => x.GradeName == name);
            });
        }

        /// <summary>正答率から Grade を決定（必要なら既定作成）</summary>
        public async Task<MGrade?> ResolveByRateAsync(double rate)
        {
            await EnsureDefaultsAsync();

            var pct = (int)Math.Round(rate * 100.0, MidpointRounding.AwayFromZero);
            var symbol = DefaultRules.First(x => pct >= x.MinPct).Name;
            return await GetByNameAsync(symbol);
        }

        // 変更：作成時に OrderNo を末尾に付与
        public async Task<ServiceResult<Unit>> CreateAsync(GradeModel model)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async repos =>
                {
                    if (model.Id == Guid.Empty) model.Id = Guid.NewGuid();

                    var exists = await repos.Grades.Queryable()
                        .AnyAsync(x => x.GradeName == model.Name);
                    if (exists) throw new InvalidOperationException("同名の成績が既に存在します。");

                    var max = await repos.Grades.Queryable()
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

                    var exists = await repos.Grades.Queryable()
                        .AnyAsync(x => x.GradeName == model.Name && x.GradeId != model.Id);
                    if (exists) throw new InvalidOperationException("同名の成績が既に存在します。");

                    // OrderNo も ApplyTo で更新される（Model が保持）
                    model.ApplyTo(e);
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
                    await r.TestResults.Queryable().AnyAsync(x => x.GradeId == id));
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

        // 追加：並び順の一括保存（0..N-1 に詰め直し）
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
                        // 現在の表示順をそのまま確定（0..N-1）
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
