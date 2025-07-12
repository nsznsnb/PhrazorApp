using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.ViewModel;

namespace PhrazorApp.Services
{
    public interface ICategoryService
    {
        Task<List<LargeCategoryModel>> GetCategoryViewModelListAsync();


    }


    /// <summary>
    /// カテゴリ情報サービス。
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly IDbContextFactory<EngDbContext> _dbContextFactory;
        
        public CategoryService(IDbContextFactory<EngDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        /// <summary>
        /// カテゴリ情報を取得します。
        /// </summary>
        /// <returns></returns>
        public async Task<List<LargeCategoryModel>> GetCategoryViewModelListAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var categories = await context.MLargeCategories
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

    }
}
