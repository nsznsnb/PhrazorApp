using PhrazorApp.Commons;
using PhrazorApp.Data.Repositories;
using PhrazorApp.Models;

namespace PhrazorApp.Services
{
    /// <summary>
    /// ジャンルサービスインターフェース
    /// </summary>
    public interface IGenreService
    {
        Task<List<GenreModel>> GetGenreViewModelListAsync();

        public Task<List<DropItemModel>> GetGenreDropItemModelListAsync();
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
        private readonly ILogger<GenreService> _logger;
        private readonly string MSG_PREFIX = "ジャンル";

        public GenreService(IGenreRepository genreRepository, ILogger<GenreService> logger)
        {
            _genreRepository = genreRepository;
            _logger = logger;
        }

        /// <summary>
        /// ジャンル情報を取得します。
        /// </summary>
        /// <returns>ジャンルのリスト</returns>
        public async Task<List<GenreModel>> GetGenreViewModelListAsync()
        {
            var genres = await _genreRepository.GetAllGenresAsync();

            // マッピング処理
            return genres.Select(x => x.ToModel()).ToList();
        }

        /// <summary>
        /// ジャンルDropItemModel一覧を取得します。
        /// </summary>
        /// <returns></returns>
        public async Task<List<DropItemModel>> GetGenreDropItemModelListAsync()
        {
            var genres = await _genreRepository.GetAllGenresAsync();
            return genres.ToDropItemModelList();
        }

        /// <summary>
        /// 単一のジャンル情報を取得します。
        /// </summary>
        /// <param name="genreId">ジャンルID</param>
        /// <returns>ジャンル情報</returns>
        public async Task<GenreModel> GetGenreViewModelAsync(Guid genreId)
        {
            var genre = await _genreRepository.GetGenreByIdAsync(genreId);

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
            var sysDateTime = DateTime.Now;

            // モデル → エンティティ
            var genreEntity = model.ToEntity();

            try
            {
                // トランザクション開始
                await _genreRepository.CreateGenreAsync(genreEntity);

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
            var sysDateTime = DateTime.Now;

            // モデル → エンティティ
            var genreEntity = model.ToEntity();

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
