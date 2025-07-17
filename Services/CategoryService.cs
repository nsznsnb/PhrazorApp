using Azure;
using Microsoft.EntityFrameworkCore;
using PhrazorApp.Common;
using PhrazorApp.Data;
using PhrazorApp.Models;
using PhrazorApp.ViewModel;

namespace PhrazorApp.Services
{
    public interface ICategoryService
    {
        public Task<List<LargeCategoryModel>> GetCategoryViewModelListAsync();

        public Task<LargeCategoryModel> GetCategoryViewModelAsync(Guid largeCategoryId);

        public Task<IServiceResult> CreateCategoryAsync(LargeCategoryModel model);

        //Task<string> UpdateCategoryAsync(LargeCategoryModel model);

    }


    /// <summary>
    /// カテゴリ情報サービス。
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly IDbContextFactory<EngDbContext> _dbContextFactory;
        private readonly IUserService _userService;

        public CategoryService(IDbContextFactory<EngDbContext> dbContextFactory, IUserService userService)
        {
            _dbContextFactory = dbContextFactory;
            _userService = userService;
        }

        /// <summary>
        /// カテゴリ情報を取得します。
        /// </summary>
        /// <returns></returns>
        public async Task<List<LargeCategoryModel>> GetCategoryViewModelListAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userId = _userService.GetUserId();

            var categories = await context.MLargeCategories
                .Where(x => x.UserId == userId || x.UserId == null)
                .Include(c => c.MSmallCategories)
                .ToListAsync();

            return categories.Select(c => new LargeCategoryModel
            {
                Id = c.LargeId,
                Name = c.LargeCategoryName,
                SubCategories = c.MSmallCategories.Select(sc => new SmallCategoryModel
                {
                    Id = sc.SmallId,
                    Name = sc.SmallCategoryName
                }).ToList()
            }).ToList();
        }

        /// <summary>
        /// カテゴリ情報を取得します。
        /// </summary>
        /// <returns></returns>
        public async Task<LargeCategoryModel> GetCategoryViewModelAsync(Guid largeCategoryId)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var userId = _userService.GetUserId();

            var largeCategory = await context.MLargeCategories
                .Include(c => c.MSmallCategories)
                .FirstOrDefaultAsync(x => x.LargeId == largeCategoryId);

            if (largeCategory == null) return new();

            var largeModel = new LargeCategoryModel
            {
                Id = largeCategoryId,
                Name = largeCategory.LargeCategoryName,
                SubCategories = largeCategory.MSmallCategories.Select(x => new SmallCategoryModel
                {
                    Id = x.SmallId,
                    Name = x.SmallCategoryName,
                }).ToList()
            };

            return largeModel;
        }


        /// <summary>
        /// カテゴリ新規作成
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IServiceResult> CreateCategoryAsync(LargeCategoryModel model)
        {

            var userId = _userService.GetUserId();
            var sysDateTime = DateTime.Now;

            var largeCategoryEntity = new MLargeCategory();
            largeCategoryEntity.LargeId = model.Id;
            largeCategoryEntity.LargeCategoryName = model.Name;
            largeCategoryEntity.CreatedAt = sysDateTime;
            largeCategoryEntity.UpdatedAt = sysDateTime;
            largeCategoryEntity.UserId = userId;

            var smallCategoryEntities = new List<MSmallCategory>();
            foreach (var smallModel in model.SubCategories)
            {
                var smallCategoryEntity = new MSmallCategory();
                smallCategoryEntity.LargeId = model.Id;
                smallCategoryEntity.SmallId = smallModel.Id;
                smallCategoryEntity.SmallCategoryName = smallModel.Name;
                smallCategoryEntity.CreatedAt = sysDateTime;
                smallCategoryEntity.UpdatedAt = sysDateTime;
                smallCategoryEntity.UserId = userId;
                smallCategoryEntities.Add(smallCategoryEntity);
            }

            await using var context = await _dbContextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                context.MLargeCategories.Add(largeCategoryEntity);
                context.MSmallCategories.AddRange(smallCategoryEntities);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure(string.Format(ComMessage.MSG_E_ERROR_CREATE_DETAIL, model.Name));
            }


            return ServiceResult.Success(string.Format(ComMessage.MSG_I_SUCCESS_DELETE_DETAIL, model.Name));
        }
    }
}
