using PhrazorApp.Models;
using PhrazorApp.Models.Mappings;
using PhrazorApp.Data.UnitOfWork;

namespace PhrazorApp.Services
{
    /// <summary>
    /// ジャンルのユースケースを提供するアプリケーションサービス。
    /// </summary>
    public sealed class GenreService
    {
        private readonly UnitOfWork _uow;
        private readonly UserService _userService;
        private readonly ILogger<GenreService> _logger;
        private const string MSG_PREFIX = "ジャンル";

        public GenreService(UnitOfWork uow, ILogger<GenreService> logger, UserService userService)
        {
            _uow = uow;
            _logger = logger;
            _userService = userService;
        }

        /// <summary>ジャンル一覧</summary>
        public Task<ServiceResult<List<GenreModel>>> GetGenreViewModelListAsync(CancellationToken ct = default)
        {
            var userId = _userService.GetUserId();
            return _uow.ReadAsync(async (u, token) =>
            {
                var genres = await u.Genres.GetAllGenresAsync(userId, token);
                var list = genres.Select(x => x.ToModel()).ToList();
                return ServiceResult.Success(list, message: ""); 
            }, ct);
        }

        /// <summary>ドロップダウン用一覧</summary>
        public Task<ServiceResult<List<DropItemModel>>> GetGenreDropItemModelListAsync(CancellationToken ct = default)
        {
            var userId = _userService.GetUserId();
            return _uow.ReadAsync(async (u, token) =>
            {
                var genres = await u.Genres.GetAllGenresAsync(userId, token);
                var list = genres.ToDropItemModelList();
                return ServiceResult.Success(list, message: "");
            }, ct);
        }

        /// <summary>ジャンル詳細</summary>
        public Task<ServiceResult<GenreModel>> GetGenreViewModelAsync(Guid genreId, CancellationToken ct = default)
        {
            var userId = _userService.GetUserId();
            return _uow.ReadAsync(async (u, token) =>
            {
                var genre = await u.Genres.GetGenreByIdAsync(genreId, userId, token); 
                var model = genre != null ? genre.ToModel() : new GenreModel();
                return ServiceResult.Success(model, message: "");
            }, ct);
        }

        /// <summary>ジャンルの新規作成</summary>
        public async Task<ServiceResult> CreateGenreAsync(GenreModel model, CancellationToken ct = default)
        {
            var userId = _userService.GetUserId();
            var entity = model.ToEntity(userId);

            try
            {
                await _uow.ExecuteInTransactionAsync(async (u, token) =>
                {
                    await u.Genres.AddAsync(entity);
                }, ct);

                return ServiceResult.Success(string.Format(AppMessages.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "ジャンル作成エラー");
                return ServiceResult.Failure(string.Format(AppMessages.MSG_E_FAILURE_CREATE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>ジャンルの更新（子サブジャンルは全入れ替え）</summary>
        public async Task<ServiceResult> UpdateGenreAsync(GenreModel model, CancellationToken ct = default)
        {
            var userId = _userService.GetUserId();
            var incoming = model.ToEntity(userId);

            try
            {
                await _uow.ExecuteInTransactionAsync(async (u, token) =>
                {
                    // ※ Repo: GetGenreByIdAsync は MSubGenres を Include しておく
                    var old = await u.Genres.GetGenreByIdAsync(incoming.GenreId, userId, token);
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
                }, ct);

                return ServiceResult.Success(string.Format(AppMessages.MSG_I_SUCCESS_UPDATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "ジャンル更新エラー");
                return ServiceResult.Failure(string.Format(AppMessages.MSG_E_FAILURE_UPDATE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>ジャンルの削除（子サブジャンルも削除）</summary>
        public async Task<ServiceResult> DeleteGenreAsync(Guid genreId, CancellationToken ct = default)
        {
            var userId = _userService.GetUserId();

            try
            {
                await _uow.ExecuteInTransactionAsync(async (u, token) =>
                {
                    var genre = await u.Genres.GetGenreByIdAsync(genreId, userId, token);
                    if (genre == null)
                        throw new InvalidOperationException(string.Format(AppMessages.MSG_E_NOT_FOUND, "指定されたジャンル"));

                    if (genre.MSubGenres is { Count: > 0 })
                        await u.SubGenres.DeleteRangeAsync(genre.MSubGenres);

                    await u.Genres.DeleteAsync(genre);
                }, ct);

                return ServiceResult.Success(string.Format(AppMessages.MSG_I_SUCCESS_DELETE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "ジャンル削除エラー");
                return ServiceResult.Failure(string.Format(AppMessages.MSG_E_FAILURE_DELETE_DETAIL, MSG_PREFIX));
            }
        }
    }
}
