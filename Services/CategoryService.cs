using PhrazorApp.Common;
using PhrazorApp.Data.Repositories;
using PhrazorApp.Models.ViewModels;

namespace PhrazorApp.Services
{
    public interface ICategoryService
    {
        Task<List<LargeCategoryModel>> GetCategoryViewModelListAsync();
        Task<LargeCategoryModel> GetCategoryViewModelAsync(Guid largeCategoryId);
        Task<IServiceResult> CreateCategoryAsync(LargeCategoryModel model);
        Task<IServiceResult> UpdateCategoryAsync(LargeCategoryModel model);
        Task<IServiceResult> DeleteCategoryAsync(Guid largeCategoryId);
    }

    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserService _userService;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepository categoryRepository, IUserService userService, ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// カテゴリ情報を取得します
        /// </summary>
        /// <returns>カテゴリのリスト</returns>
        public async Task<List<LargeCategoryModel>> GetCategoryViewModelListAsync()
        {
            var userId = _userService.GetUserId();
            var categories = await _categoryRepository.GetAllCategoriesAsync(userId);

            // マッピング処理
            return categories.Select(x => x.ToModel()).ToList();
        }

        /// <summary>
        /// 単一のカテゴリ情報を取得します
        /// </summary>
        /// <param name="largeCategoryId">カテゴリID</param>
        /// <returns>カテゴリ情報</returns>
        public async Task<LargeCategoryModel> GetCategoryViewModelAsync(Guid largeCategoryId)
        {
            var userId = _userService.GetUserId();
            var category = await _categoryRepository.GetCategoryByIdAsync(largeCategoryId, userId);

            // マッピング処理
            return category != null ? category.ToModel() : new LargeCategoryModel();
        }

        /// <summary>
        /// 新しいカテゴリを作成します
        /// </summary>
        /// <param name="model">カテゴリモデル</param>
        /// <returns>操作結果</returns>
        public async Task<IServiceResult> CreateCategoryAsync(LargeCategoryModel model)
        {
            var userId = _userService.GetUserId();
            var sysDateTime = DateTime.Now;

            // モデル → エンティティ
            var largeCategoryEntity = model.ToEntity(userId, sysDateTime);
            var smallCategoryEntities = model.ToSmallEntities(userId, sysDateTime);

            try
            {
                // トランザクション開始
                await _categoryRepository.AddCategoriesWithTransactionAsync(largeCategoryEntity, smallCategoryEntities);

                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_CREATE_DETAIL, model.Name));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "カテゴリ作成エラー");
                return ServiceResult.Failure("カテゴリ作成に失敗しました");
            }
        }

        /// <summary>
        /// カテゴリ情報を更新します
        /// </summary>
        /// <param name="model">更新するカテゴリモデル</param>
        /// <returns>操作結果</returns>
        public async Task<IServiceResult> UpdateCategoryAsync(LargeCategoryModel model)
        {
            var userId = _userService.GetUserId();
            var sysDateTime = DateTime.Now;

            // モデル → エンティティ
            var largeCategoryEntity = model.ToEntity(userId, sysDateTime);

            try
            {
                // 更新
                await _categoryRepository.UpdateCategoryAsync(largeCategoryEntity);

                return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_UPDATE_DETAIL, model.Name));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "カテゴリ更新エラー");
                return ServiceResult.Failure("カテゴリ更新に失敗しました");
            }
        }

        /// <summary>
        /// カテゴリを削除します
        /// </summary>
        /// <param name="largeCategoryId">削除するカテゴリID</param>
        /// <returns>操作結果</returns>
        public async Task<IServiceResult> DeleteCategoryAsync(Guid largeCategoryId)
        {
            try
            {
                await _categoryRepository.DeleteCategoryAsync(largeCategoryId);

                return ServiceResult.Success("カテゴリ削除成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "カテゴリ削除エラー");
                return ServiceResult.Failure("カテゴリ削除に失敗しました");
            }
        }
    }
}
