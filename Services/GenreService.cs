using PhrazorApp.Common;
using PhrazorApp.Data.Repositories;
using PhrazorApp.Models.ViewModels;

namespace PhrazorApp.Services
{
    /// <summary>
    /// ジャンルサービスインターフェース
    /// </summary>
    public interface IGenreService
    {
        Task<List<GenreModel>> GetGenreViewModelListAsync();
        Task<GenreModel> GetGenreViewModelAsync(Guid genreId);
        Task<IServiceResult> CreateGenreAsync(GenreModel model);
        Task<IServiceResult> UpdateGenreAsync(GenreModel model);
        Task<IServiceResult> DeleteGenreAsync(Guid genreId);
    }

    /// <summary>
    /// ジャンルサービス
    /// </summary>
    public class GenreService : IGenreService
    {
        private readonly IGenreRepository _genreRepository;
        private readonly IUserService _userService;
        private readonly ILogger<GenreService> _logger;
        private readonly string MSG_PREFIX = "ジャンル";

        public GenreService(IGenreRepository genreRepository, IUserService userService, ILogger<GenreService> logger)
        {
            _genreRepository = genreRepository;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// ジャンル情報を取得します。
        /// </summary>
        /// <returns>ジャンルのリスト</returns>
        public async Task<List<GenreModel>> GetGenreViewModelListAsync()
        {
            var userId = _userService.GetUserId();
            var genres = await _genreRepository.GetAllGenresAsync(userId);

            // マッピング処理
            return genres.Select(x => x.ToModel()).ToList();
        }

        /// <summary>
        /// 単一のジャンル情報を取得します。
        /// </summary>
        /// <param name="genreId">ジャンルID</param>
        /// <returns>ジャンル情報</returns>
        public async Task<GenreModel> GetGenreViewModelAsync(Guid genreId)
        {
            var userId = _userService.GetUserId();
            var genre = await _genreRepository.GetGenreByIdAsync(genreId, userId);

            // マッピング処理
            return genre != null ? genre.ToModel() : new GenreModel();
        }

        /// <summary>
        /// 新しいジャンルを作成します。
        /// </summary>
        /// <param name="model">ジャンルモデル</param>
        /// <returns>操作結果</returns>
        public async Task<IServiceResult> CreateGenreAsync(GenreModel model)
        {
            var userId = _userService.GetUserId();
            var sysDateTime = DateTime.Now;

            // モデル → エンティティ
            var genreEntity = model.ToEntity(userId, sysDateTime);
            var subGenreEntities = model.ToSubGenreEntities(userId, sysDateTime);

            try
            {
                // トランザクション開始
                await _genreRepository.CreateGenresWithTransactionAsync(genreEntity, subGenreEntities);

                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_CREATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ジャンル作成エラー");
                return ServiceResult.Failure(string.Format(ComMessage.MSG_E_FAILURE_CREATE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>
        /// ジャンル情報を更新します。
        /// </summary>
        /// <param name="model">更新するジャンルモデル</param>
        /// <returns>操作結果</returns>
        public async Task<IServiceResult> UpdateGenreAsync(GenreModel model)
        {
            var userId = _userService.GetUserId();
            var sysDateTime = DateTime.Now;

            // モデル → エンティティ
            var genreEntity = model.ToEntity(userId, sysDateTime);

            try
            {
                // 更新
                await _genreRepository.UpdateGenreAsync(genreEntity);

                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_UPDATE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ジャンル更新エラー");
                return ServiceResult.Failure(string.Format(ComMessage.MSG_E_FAILURE_UPDATE_DETAIL, MSG_PREFIX));
            }
        }

        /// <summary>
        /// ジャンルを削除します。
        /// </summary>
        /// <param name="genreId">削除するジャンルID</param>
        /// <returns>操作結果</returns>
        public async Task<IServiceResult> DeleteGenreAsync(Guid genreId)
        {
            try
            {
                await _genreRepository.DeleteGenreAsync(genreId);

                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_DELETE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ジャンル削除エラー");
                return ServiceResult.Failure(string.Format(ComMessage.MSG_E_FAILURE_DELETE_DETAIL, MSG_PREFIX));
            }
        }
    }
}
