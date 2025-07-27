using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{

    public interface ICategoryRepository
    {
        Task AddCategoryAsync(MLargeCategory category);
        Task AddSmallCategoryAsync(MSmallCategory smallCategory);
        Task AddCategoriesWithTransactionAsync(MLargeCategory largeCategory, List<MSmallCategory> smallCategories);
        Task<List<MLargeCategory>> GetAllCategoriesAsync(string? userId);
        Task<MLargeCategory?> GetCategoryByIdAsync(Guid categoryId, string? userId);
        Task UpdateCategoryAsync(MLargeCategory category);
        Task DeleteCategoryAsync(Guid categoryId);
    }

    public class CategoryRepository : ICategoryRepository
    {
        private readonly IDbContextFactory<EngDbContext> _dbContextFactory;

        public CategoryRepository(IDbContextFactory<EngDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        /// <summary>
        /// 単一の大カテゴリを追加します。
        /// </summary>
        /// <param name="category">追加する大カテゴリ</param>
        public async Task AddCategoryAsync(MLargeCategory category)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            await context.MLargeCategories.AddAsync(category);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 単一の小カテゴリを追加します。
        /// </summary>
        /// <param name="smallCategory">追加する小カテゴリ</param>
        public async Task AddSmallCategoryAsync(MSmallCategory smallCategory)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            await context.MSmallCategories.AddAsync(smallCategory);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 大カテゴリとその小カテゴリを一度に追加します。
        /// トランザクションを使用して一貫性を確保します。
        /// </summary>
        /// <param name="largeCategory">追加する大カテゴリ</param>
        /// <param name="smallCategories">追加する小カテゴリのリスト</param>
        public async Task AddCategoriesWithTransactionAsync(MLargeCategory largeCategory, List<MSmallCategory> smallCategories)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // 大カテゴリを追加
                await context.MLargeCategories.AddAsync(largeCategory);
                // 小カテゴリを追加
                if (smallCategories != null && smallCategories.Count > 0)
                {
                    await context.MSmallCategories.AddRangeAsync(smallCategories);
                }

                // 保存してトランザクションをコミット
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("カテゴリの作成中にエラーが発生しました", ex);
            }
        }

        /// <summary>
        /// ユーザーIDに基づいて、すべての大カテゴリを取得します。
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>大カテゴリのリスト</returns>
        public async Task<List<MLargeCategory>> GetAllCategoriesAsync(string? userId)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.MLargeCategories
                .Where(x => x.UserId == userId)
                .Include(x => x.MSmallCategories)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// 指定された大カテゴリIDに基づいて、大カテゴリを取得します。
        /// </summary>
        /// <param name="categoryId">大カテゴリID</param>
        /// <param name="userId">ユーザーID</param>
        /// <returns>指定された大カテゴリ</returns>
        public async Task<MLargeCategory?> GetCategoryByIdAsync(Guid categoryId, string? userId)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.MLargeCategories
                .Include(x => x.MSmallCategories)
                .FirstOrDefaultAsync(x => x.LargeId == categoryId && x.UserId == userId);
        }

        /// <summary>
        /// 大カテゴリを更新します。
        /// </summary>
        /// <param name="category">更新する大カテゴリ</param>
        public async Task UpdateCategoryAsync(MLargeCategory category)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            context.MLargeCategories.Update(category);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 大カテゴリを削除します。
        /// その関連する小カテゴリも一緒に削除します。
        /// </summary>
        /// <param name="categoryId">削除する大カテゴリID</param>
        public async Task DeleteCategoryAsync(Guid categoryId)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var category = await context.MLargeCategories
                .Include(x => x.MSmallCategories)
                .FirstOrDefaultAsync(x => x.LargeId == categoryId);

            if (category != null)
            {
                // 小カテゴリを削除
                context.MSmallCategories.RemoveRange(category.MSmallCategories);
                // 大カテゴリを削除
                context.MLargeCategories.Remove(category);
                await context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("指定された大カテゴリが存在しません");
            }
        }
    }

}
