using PhrazorApp.Commons;
using PhrazorApp.Models;
using PhrazorApp.Models.Mappings;
using PhrazorApp.Data.UnitOfWork;

namespace PhrazorApp.Services
{
    public class GenreService
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

        public Task<List<GenreModel>> GetGenreViewModelListAsync()
        {
            var userId = _userService.GetUserId();
            return _uow.ReadAsync(async u =>
            {
                var genres = await u.Genres.GetAllGenresAsync(userId);
                return genres.Select(x => x.ToModel()).ToList();
            });
        }

        public Task<List<DropItemModel>> GetGenreDropItemModelListAsync()
        {
            var userId = _userService.GetUserId();
            return _uow.ReadAsync(async u =>
            {
                var genres = await u.Genres.GetAllGenresAsync(userId);
                return genres.ToDropItemModelList();
            });
        }

        public Task<GenreModel> GetGenreViewModelAsync(Guid genreId)
        {
            var userId = _userService.GetUserId();
            return _uow.ReadAsync(async u =>
            {
                var genre = await u.Genres.GetGenreByIdAsync(genreId, userId);
                return genre != null ? genre.ToModel() : new GenreModel();
            });
        }

        public async Task<IServiceResult> CreateGenreAsync(GenreModel model)
        {
            var userId = _userService.GetUserId();
            var entity = model.ToEntity(userId);

            try
            {
                await _uow.ExecuteInTransactionAsync(async u =>
                {
                    await u.Genres.AddAsync(entity);
                });

                return ServiceResult.Success(string.Format(AppMessages.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ジャンル作成エラー");
                return ServiceResult.Failure(string.Format(AppMessages.MSG_E_FAILURE_CREATE_DETAIL, MSG_PREFIX));
            }
        }

        public async Task<IServiceResult> UpdateGenreAsync(GenreModel model)
        {
            var userId = _userService.GetUserId();
            var incoming = model.ToEntity(userId);

            try
            {
                await _uow.ExecuteInTransactionAsync(async u =>
                {
                    var old = await u.Genres.GetGenreByIdAsync(incoming.GenreId, userId);
                    if (old == null)
                        throw new InvalidOperationException("対象のジャンルが見つかりません。");

                    if (old.MSubGenres is { Count: > 0 })
                        await u.SubGenres.DeleteRangeAsync(old.MSubGenres);

                    if (incoming.MSubGenres?.Any() == true)
                    {
                        foreach (var sg in incoming.MSubGenres) sg.GenreId = incoming.GenreId;
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

        public async Task<IServiceResult> DeleteGenreAsync(Guid genreId)
        {
            var userId = _userService.GetUserId();

            try
            {
                await _uow.ExecuteInTransactionAsync(async u =>
                {
                    var genre = await u.Genres.GetGenreByIdAsync(genreId, userId);
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
    }
}
