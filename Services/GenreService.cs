using PhrazorApp.Data.Entities;
using PhrazorApp.Data.UnitOfWork;
using PhrazorApp.Models;
using PhrazorApp.Models.Mappings;

namespace PhrazorApp.Services
{
    /// <summary>
    /// ジャンルのユースケースを提供するアプリケーションサービス（CT不使用版）。
    /// </summary>
    public sealed class GenreService
    {
        private readonly UnitOfWork _uow;
        private readonly UserService _userService;
        private readonly ILogger<GenreService> _logger;
        private const string MSG_PREFIX = "ジャンル";

        // 既定サブジャンル名
        private const string DEFAULT_SUBGENRE_NAME = "未分類";

        public GenreService(UnitOfWork uow, ILogger<GenreService> logger, UserService userService)
        {
            _uow = uow;
            _logger = logger;
            _userService = userService;
        }

        /// <summary>ジャンル一覧</summary>
        public Task<ServiceResult<List<GenreModel>>> GetGenreViewModelListAsync()
        {
            return _uow.ReadAsync(async u =>
            {
                var genres = await u.Genres.GetAllGenresAsync();
                var list = genres.Select(x => x.ToModel()).ToList();
                return ServiceResult.Success(list, message: "");
            });
        }

        /// <summary>ドロップダウン用一覧</summary>
        public Task<ServiceResult<List<DropItemModel>>> GetGenreDropItemModelListAsync()
        {
            return _uow.ReadAsync(async u =>
            {
                var genres = await u.Genres.GetAllGenresAsync();
                var list = genres.ToDropItemModelList();
                return ServiceResult.Success(list, message: "");
            });
        }

        /// <summary>ジャンル詳細</summary>
        public Task<ServiceResult<GenreModel>> GetGenreViewModelAsync(Guid genreId)
        {
            return _uow.ReadAsync(async u =>
            {
                var genre = await u.Genres.GetGenreByIdAsync(genreId);
                var model = genre != null ? genre.ToModel() : new GenreModel();
                return ServiceResult.Success(model, message: "");
            });
        }

        /// <summary>ジャンルの新規作成（サブジャンル未指定なら既定を1件付与）</summary>
        public async Task<ServiceResult> CreateGenreAsync(GenreModel model)
        {
            var userId = _userService.GetUserId();
            var entity = model.ToEntity(userId);

            // サブジャンルが無ければ既定を付与
            EnsureOneDefaultSubGenre(entity, userId);

            try
            {
                await _uow.ExecuteInTransactionAsync(async u =>
                {
                    await u.Genres.AddAsync(entity);   // MSubGenres がセットされていれば一括で追加
                });

                return ServiceResult.Success(string.Format(AppMessages.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ジャンル作成エラー");
                return ServiceResult.Failure(string.Format(AppMessages.MSG_E_FAILURE_CREATE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>ジャンルの更新（子サブジャンルは全入れ替え、Default を必ず1件維持）</summary>
        public async Task<ServiceResult> UpdateGenreAsync(GenreModel model)
        {
            var userId = _userService.GetUserId();
            var incoming = model.ToEntity(userId);

            // 受け取ったサブジャンルの整形（ID補完・SortOrder正規化・Defaultを1件に）
            EnsureOneDefaultSubGenre(incoming, userId);

            try
            {
                await _uow.ExecuteInTransactionAsync(async u =>
                {
                    // ※ Repo: GetGenreByIdAsync は MSubGenres を Include 済み想定
                    var old = await u.Genres.GetGenreByIdAsync(incoming.GenreId);
                    if (old == null)
                        throw new InvalidOperationException("対象のジャンルが見つかりません。");

                    if (old.MSubGenres is { Count: > 0 })
                        await u.SubGenres.DeleteRangeAsync(old.MSubGenres);

                    if (incoming.MSubGenres?.Any() == true)
                    {
                        foreach (var sg in incoming.MSubGenres)
                            sg.GenreId = incoming.GenreId;

                        await u.SubGenres.AddRangeAsync(incoming.MSubGenres);
                    }

                    old.GenreName = incoming.GenreName;
                    await u.Genres.UpdateAsync(old);
                });

                return ServiceResult.Success(string.Format(AppMessages.MSG_I_SUCCESS_UPDATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ジャンル更新エラー");
                return ServiceResult.Failure(string.Format(AppMessages.MSG_E_FAILURE_UPDATE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>ジャンルの削除（子サブジャンルも削除）</summary>
        public async Task<ServiceResult> DeleteGenreAsync(Guid genreId)
        {
            try
            {
                await _uow.ExecuteInTransactionAsync(async u =>
                {
                    var genre = await u.Genres.GetGenreByIdAsync(genreId);
                    if (genre == null)
                        throw new InvalidOperationException(string.Format(AppMessages.MSG_E_NOT_FOUND, "指定されたジャンル"));

                    if (genre.MSubGenres is { Count: > 0 })
                        await u.SubGenres.DeleteRangeAsync(genre.MSubGenres);

                    await u.Genres.DeleteAsync(genre);
                });

                return ServiceResult.Success(string.Format(AppMessages.MSG_I_SUCCESS_DELETE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ジャンル削除エラー");
                return ServiceResult.Failure(string.Format(AppMessages.MSG_E_FAILURE_DELETE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>
        /// サブジャンルを整形し、必ず DefaultFlg=true が1件になるように補正。
        /// 無い場合は既定のサブジャンル（未分類）を自動作成。
        /// </summary>
        private static void EnsureOneDefaultSubGenre(
            MGenre genre,
            string userId,
            string defaultName = DEFAULT_SUBGENRE_NAME)
        {
            // null → 空コレクション化
            genre.MSubGenres ??= new List<MSubGenre>();

            // 空なら既定を1件作成
            if (genre.MSubGenres.Count == 0)
            {
                genre.MSubGenres.Add(new MSubGenre
                {
                    SubGenreId = Guid.NewGuid(),
                    GenreId = genre.GenreId,
                    SubGenreName = defaultName,
                    OrderNo = 0,
                    IsDefault = true,
                    UserId = userId
                });
                return;
            }

            // ID未設定の補完・GenreId/UserId の付与・名称の穴埋め
            foreach (var sg in genre.MSubGenres)
            {
                if (sg.SubGenreId == Guid.Empty) sg.SubGenreId = Guid.NewGuid();
                sg.GenreId = genre.GenreId;
                sg.UserId = userId;
                if (string.IsNullOrWhiteSpace(sg.SubGenreName))
                    sg.SubGenreName = defaultName;
            }

            // 既定をちょうど1件に（0件→先頭を既定、2件以上→先頭以外を解除）
            var defaults = genre.MSubGenres.Where(x => x.IsDefault).ToList();
            if (defaults.Count == 0)
                genre.MSubGenres.First().IsDefault = true;
            else if (defaults.Count > 1)
                foreach (var d in defaults.Skip(1)) d.IsDefault = false;

            // OrderNo を 0..N-1 に詰め直し（現在の順序→名前で安定化）
            var normalized = genre.MSubGenres
                .OrderBy(x => x.OrderNo)
                .ThenBy(x => x.SubGenreName)
                .ToList();

            for (int i = 0; i < normalized.Count; i++)
                normalized[i].OrderNo = i;

            genre.MSubGenres = normalized;
        }
    }
}
