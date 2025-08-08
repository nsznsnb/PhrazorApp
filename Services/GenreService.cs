using PhrazorApp.Commons;
using PhrazorApp.Models;
using PhrazorApp.Models.Mappings;
using PhrazorApp.Data.UnitOfWork;

namespace PhrazorApp.Services
{
    public class GenreService
    {
        private readonly IUnitOfWork _uow;
        private readonly UserService _userService;
        private readonly ILogger<GenreService> _logger;
        private const string MSG_PREFIX = "ジャンル";

        public GenreService(IUnitOfWork uow, ILogger<GenreService> logger, UserService userService)
        {
            _uow = uow;
            _logger = logger;
            _userService = userService;
        }

        public async Task<List<GenreModel>> GetGenreViewModelListAsync(CancellationToken ct = default)
        {
            var userId = _userService.GetUserId();
            await _uow.BeginAsync(ct);
            var genres = await _uow.Genres.GetAllGenresAsync(userId, ct);
            return genres.Select(x => x.ToModel()).ToList();
        }

        public async Task<List<DropItemModel>> GetGenreDropItemModelListAsync(CancellationToken ct = default)
        {
            var userId = _userService.GetUserId();
            await _uow.BeginAsync(ct);
            var genres = await _uow.Genres.GetAllGenresAsync(userId, ct);
            return genres.ToDropItemModelList();
        }

        public async Task<GenreModel> GetGenreViewModelAsync(Guid genreId, CancellationToken ct = default)
        {
            var userId = _userService.GetUserId();
            await _uow.BeginAsync(ct);
            var genre = await _uow.Genres.GetGenreByIdAsync(genreId, userId, ct);
            return genre != null ? genre.ToModel() : new GenreModel();
        }

        public async Task<IServiceResult> CreateGenreAsync(GenreModel model, CancellationToken ct = default)
        {
            var userId = _userService.GetUserId();
            var genreEntity = model.ToEntity(userId);

            try
            {
                await _uow.BeginAsync(ct);
                await _uow.Genres.AddAsync(genreEntity);
                await _uow.CommitAsync(ct);
                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.LogError(ex, "ジャンル作成エラー");
                return ServiceResult.Failure(string.Format(ComMessage.MSG_E_FAILURE_CREATE_DETAIL, MSG_PREFIX));
            }
        }

        public async Task<IServiceResult> UpdateGenreAsync(GenreModel model, CancellationToken ct = default)
        {
            var userId = _userService.GetUserId();
            var genre = model.ToEntity(userId);

            try
            {
                await _uow.BeginAsync(ct);
                var old = await _uow.Genres.GetGenreByIdAsync(genre.GenreId, userId, ct);
                if (old?.MSubGenres is { Count: > 0 })
                    await _uow.SubGenres.DeleteRangeAsync(old.MSubGenres);

                if (genre.MSubGenres?.Any() == true)
                    await _uow.SubGenres.AddRangeAsync(genre.MSubGenres);

                await _uow.Genres.UpdateAsync(genre);
                await _uow.CommitAsync(ct);
                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_UPDATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.LogError(ex, "ジャンル更新エラー");
                return ServiceResult.Failure(string.Format(ComMessage.MSG_E_FAILURE_UPDATE_DETAIL, MSG_PREFIX));
            }
        }

        public async Task<IServiceResult> DeleteGenreAsync(Guid genreId, CancellationToken ct = default)
        {
            var userId = _userService.GetUserId();

            try
            {
                await _uow.BeginAsync(ct);
                var genre = await _uow.Genres.GetGenreByIdAsync(genreId, userId, ct);
                if (genre == null)
                    return ServiceResult.Failure(string.Format(ComMessage.MSG_E_NOT_FOUND, "指定されたジャンル"));

                if (genre.MSubGenres is { Count: > 0 })
                    await _uow.SubGenres.DeleteRangeAsync(genre.MSubGenres);

                await _uow.Genres.DeleteAsync(genre);
                await _uow.CommitAsync(ct);
                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_DELETE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync(ct);
                _logger.LogError(ex, "ジャンル削除エラー");
                return ServiceResult.Failure(string.Format(ComMessage.MSG_E_FAILURE_DELETE_DETAIL, MSG_PREFIX));
            }
        }
    }
}
