using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data.UnitOfWork;
using PhrazorApp.Models;
using PhrazorApp.Models.Mappings;

namespace PhrazorApp.Services
{
    /// <summary>日記タグのユースケース</summary>
    public sealed class DiaryTagService
    {
        private readonly UnitOfWork _uow;
        private readonly UserService _user;
        private readonly ILogger<DiaryTagService> _log;

        private const string MSG_PREFIX = "日記タグ";

        public DiaryTagService(UnitOfWork uow, UserService user, ILogger<DiaryTagService> log)
        {
            _uow = uow;
            _user = user;
            _log = log;
        }

        /// <summary>一覧取得（ユーザー単位／名前昇順）</summary>
        public Task<ServiceResult<List<DiaryTagModel>>> GetListAsync()
        {
            return _uow.ReadAsync(async repos =>
            {
                // グローバルフィルタが有効なら UserId 条件は省略可（冪等のため残してもOK）
                var uid = _user.GetUserId();
                var list = await repos.DiaryTags
                    .Queryable(asNoTracking: true)
                    .Where(x => x.UserId == uid)
                    .OrderBy(x => x.TagName)
                    .Select(DiaryTagMapper.ListProjection)
                    .ToListAsync();

                return ServiceResult.Success(list, "");
            });
        }

        /// <summary>1件取得</summary>
        public Task<ServiceResult<DiaryTagModel>> GetAsync(Guid id)
        {
            return _uow.ReadAsync(async repos =>
            {
                var uid = _user.GetUserId();
                var e = await repos.DiaryTags
                    .Queryable(asNoTracking: true)
                    .Where(x => x.UserId == uid && x.TagId == id)
                    .FirstOrDefaultAsync();

                if (e is null)
                    return ServiceResult.Error<DiaryTagModel>($"{MSG_PREFIX}が見つかりません。");

                return ServiceResult.Success(e.ToModel(), "");
            });
        }

        public Task<ServiceResult<List<DropItemModel>>> GetDiaryTagDropItemModelListAsync()
      => _uow.ReadAsync(async r =>
      {
          var uid = _user.GetUserId();
          var tags = await r.DiaryTags.GetAllAsync();

          var list = tags
              .Select(t => new DropItemModel
              {
                  Key1 = t.TagId,                  // TagId (Guid)
                  Key2 = Guid.Empty,            // タグは親なし
                  Name = t.TagName ?? string.Empty,
                  DropTarget = DropItemType.UnAssigned
              })
              .ToList();

          return ServiceResult.Success(list);
      });

        public async Task<ServiceResult<Unit>> CreateAsync(DiaryTagModel model)
        {
            var name = (model.Name ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name))
                return ServiceResult.None.Error("タグ名は必須です。");

            try
            {
                await _uow.ExecuteInTransactionAsync(async repos =>
                {
                    var uid = _user.GetUserId();

                    // ★ ここを修正：StringComparison を使用しない
                    var exists = await repos.DiaryTags
                        .Queryable()
                        .AnyAsync(x => x.UserId == uid && x.TagName == name);

                    if (exists)
                        throw new InvalidOperationException("同名のタグが既に存在します。");

                    var ent = model.ToEntityForCreate(uid, name);
                    await repos.DiaryTags.AddAsync(ent);
                });

                return ServiceResult.None.Success(string.Format(AppMessages.MSG_I_SUCCESS_CREATE_DETAIL, "日記タグ"));
            }
            catch (InvalidOperationException dupEx)
            {
                // 重複はユーザーに分かるメッセージに
                _log.LogWarning(dupEx, "DiaryTag duplicate: {Name}", model.Name);
                return ServiceResult.None.Error("同名のタグが既に存在します。");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "DiaryTag create error");
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_CREATE_DETAIL, "日記タグ"));
            }
        }

        public async Task<ServiceResult<Unit>> UpdateAsync(DiaryTagModel model)
        {
            var name = (model.Name ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name))
                return ServiceResult.None.Error("タグ名は必須です。");

            try
            {
                await _uow.ExecuteInTransactionAsync(async repos =>
                {
                    var uid = _user.GetUserId();

                    var ent = await repos.DiaryTags
                        .Queryable()
                        .FirstOrDefaultAsync(x => x.UserId == uid && x.TagId == model.Id)
                        ?? throw new InvalidOperationException("対象が見つかりません。");

                    var dup = await repos.DiaryTags
                        .Queryable()
                        .AnyAsync(x => x.UserId == uid && x.TagId != model.Id && x.TagName == name);

                    if (dup)
                        throw new InvalidOperationException("同名のタグが既に存在します。");

                    model.ApplyTo(ent, name);
                    await repos.DiaryTags.UpdateAsync(ent);
                });

                return ServiceResult.None.Success(string.Format(AppMessages.MSG_I_SUCCESS_UPDATE_DETAIL, "日記タグ"));
            }
            catch (InvalidOperationException dupEx)
            {
                _log.LogWarning(dupEx, "DiaryTag duplicate on update: {Name}", model.Name);
                return ServiceResult.None.Error("同名のタグが既に存在します。");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "DiaryTag update error");
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_UPDATE_DETAIL, "日記タグ"));
            }
        }

        /// <summary>削除</summary>
        public async Task<ServiceResult<Unit>> DeleteAsync(Guid id)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async repos =>
                {
                    var uid = _user.GetUserId();

                    var ent = await repos.DiaryTags
                        .Queryable()
                        .FirstOrDefaultAsync(x => x.UserId == uid && x.TagId == id)
                        ?? throw new InvalidOperationException("対象が見つかりません。");

                    await repos.DiaryTags.DeleteAsync(ent);
                });

                return ServiceResult.None.Success(string.Format(AppMessages.MSG_I_SUCCESS_DELETE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "DiaryTag delete error");
                return ServiceResult.None.Error(string.Format(AppMessages.MSG_E_FAILURE_DELETE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>一括削除（今後の一括運用を見据えて実装）</summary>
        public async Task<ServiceResult<Unit>> DeleteManyAsync(IReadOnlyCollection<Guid> ids)
        {
            if (ids is null || ids.Count == 0)
                return ServiceResult.None.Success("削除対象がありません。");

            try
            {
                await _uow.ExecuteInTransactionAsync(async repos =>
                {
                    var uid = _user.GetUserId();

                    var targets = await repos.DiaryTags
                        .Queryable()
                        .Where(x => x.UserId == uid && ids.Contains(x.TagId))
                        .ToListAsync();

                    if (targets.Count == 0) return;

                    await repos.DiaryTags.DeleteRangeAsync(targets);
                });

                return ServiceResult.None.Success($"{MSG_PREFIX}の一括削除を完了しました。");
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "DiaryTag bulk delete error");
                return ServiceResult.None.Error($"{MSG_PREFIX}の一括削除に失敗しました。");
            }
        }
    }
}
