using Microsoft.EntityFrameworkCore;
using PhrazorApp.Commons;
using PhrazorApp.Data;
using PhrazorApp.Data.Repositories;
using PhrazorApp.Models;

namespace PhrazorApp.Services
{

    /// <summary>
    /// ジャンルサービス
    /// </summary>
    public class GenreService
    {
        private readonly IDbContextFactory<EngDbContext> _dbContextFactory;
        private readonly UserService _userService;
        private readonly GenreRepository _genreRepository;
        private readonly SubGenreRepository _subGenreRepository;
        private readonly ILogger<GenreService> _logger;
        private readonly string MSG_PREFIX = "ジャンル";

        public GenreService(IDbContextFactory<EngDbContext> dbContextFactory,
                            GenreRepository genreRepository,
                            SubGenreRepository subGenreRepository,
                            ILogger<GenreService> logger,
                            UserService userService)
        {
            _dbContextFactory = dbContextFactory;
            _genreRepository = genreRepository;
            _subGenreRepository = subGenreRepository;
            _logger = logger;
            _userService = userService;
        }

        /// <summary>
        /// ジャンル情報を取得します。
        /// </summary>
        /// <returns>ジャンルのリスト</returns>
        public async Task<List<GenreModel>> GetGenreViewModelListAsync()
        {
            var userId = _userService.GetUserId();
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            var genres = await _genreRepository.GetAllGenresAsync(context, userId);

            // マッピング処理
            return genres.Select(x => x.ToModel()).ToList();
        }

        /// <summary>
        /// ジャンルDropItemModel一覧を取得します。
        /// </summary>
        /// <returns></returns>
        public async Task<List<DropItemModel>> GetGenreDropItemModelListAsync()
        {
            var userId = _userService.GetUserId();
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            var genres = await _genreRepository.GetAllGenresAsync(context, userId);
            return genres.ToDropItemModelList();
        }

        /// <summary>
        /// 単一のジャンル情報を取得します。
        /// </summary>
        /// <param name="genreId">ジャンルID</param>
        /// <returns>ジャンル情報</returns>
        public async Task<GenreModel> GetGenreViewModelAsync(Guid genreId)
        {
            var userId = _userService.GetUserId();
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var genre = await _genreRepository.GetGenreByIdAsync(context, genreId, userId);

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

            // モデル → エンティティ
            var genreEntity = model.ToEntity(userId);

            try
            {
                await using var context = await _dbContextFactory.CreateDbContextAsync();

                await _genreRepository.AddAsync(context, genreEntity);

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

            await using var context = await _dbContextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // エンティティ変換
                var genre = model.ToEntity(userId);

                // サブジャンルの完全差し替え
                var oldSubGenres = await context.MSubGenres
                    .Where(x => x.GenreId == genre.GenreId)
                    .ToListAsync();

                context.MSubGenres.RemoveRange(oldSubGenres);

                if (genre.MSubGenres?.Any() == true)
                {
                    await _subGenreRepository.AddRangeAsync(context, genre.MSubGenres);
                }

                await _genreRepository.UpdateAsync(context, genre);

                await transaction.CommitAsync();
                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_UPDATE_DETAIL, MSG_PREFIX));

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
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
            var userId = _userService.GetUserId();
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // 1. ジャンル取得（サブジャンル含む）
                var genre = await _genreRepository.GetGenreByIdAsync(context, genreId, userId);
                if (genre == null)
                {
                    throw new Exception(string.Format(ComMessage.MSG_E_NOT_FOUND, "指定されたジャンル"));
                }

                // 2. サブジャンル削除
                if (genre.MSubGenres?.Any() == true)
                {
                    await _subGenreRepository.DeleteRangeAsync(context, genre.MSubGenres);
                }

                // 3. ジャンル削除
                await _genreRepository.DeleteAsync(context, genre);

                await transaction.CommitAsync();
                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_DELETE_DETAIL, MSG_PREFIX));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "ジャンル削除エラー");
                return ServiceResult.Failure(string.Format(ComMessage.MSG_E_FAILURE_DELETE_DETAIL, MSG_PREFIX));
            }

        }
    }
}
