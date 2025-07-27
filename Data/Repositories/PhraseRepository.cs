using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public interface IPhraseRepository
    {
        Task<List<DPhrase>> GetAllPhrasesAsync();
        Task<DPhrase?> GetPhraseByIdAsync(Guid phraseId);
        Task AddPhraseAsync(DPhrase phrase);
        Task UpdatePhraseAsync(DPhrase phrase);
        Task DeletePhraseAsync(DPhrase phrase);
        Task AddPhraseCategoryAsync(IEnumerable<MPhraseCategory> phraseCategories);
        Task DeletePhraseCategoriesAsync(IEnumerable<MPhraseCategory> phraseCategories);
        Task AddPhraseImageAsync(DPhraseImage image);
        Task UpdatePhraseImageAsync(DPhraseImage image);
    }
    public class PhraseRepository : IPhraseRepository
    {
        private readonly IDbContextFactory<EngDbContext> _dbContextFactory;

        public PhraseRepository(IDbContextFactory<EngDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        /// <summary>
        /// すべてのフレーズを取得します。
        /// </summary>
        public async Task<List<DPhrase>> GetAllPhrasesAsync()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.DPhrases
                .Include(p => p.DPhraseImage)
                .Include(p => p.MPhraseCategories)
                .ToListAsync();
        }

        /// <summary>
        /// 指定されたフレーズIDのフレーズを取得します。
        /// </summary>
        public async Task<DPhrase?> GetPhraseByIdAsync(Guid phraseId)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.DPhrases
                .Include(p => p.DPhraseImage)
                .Include(p => p.MPhraseCategories)
                .FirstOrDefaultAsync(p => p.PhraseId == phraseId);
        }

        /// <summary>
        /// 新しいフレーズを追加します。
        /// </summary>
        public async Task AddPhraseAsync(DPhrase phrase)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            context.DPhrases.Add(phrase);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// フレーズ情報を更新します。
        /// </summary>
        public async Task UpdatePhraseAsync(DPhrase phrase)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            context.DPhrases.Update(phrase);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// フレーズを削除します。
        /// </summary>
        public async Task DeletePhraseAsync(DPhrase phrase)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            context.DPhrases.Remove(phrase);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// フレーズカテゴリを追加します。
        /// </summary>
        public async Task AddPhraseCategoryAsync(IEnumerable<MPhraseCategory> phraseCategories)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            context.MPhraseCategories.AddRange(phraseCategories);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// フレーズカテゴリを削除します。
        /// </summary>
        public async Task DeletePhraseCategoriesAsync(IEnumerable<MPhraseCategory> phraseCategories)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            context.MPhraseCategories.RemoveRange(phraseCategories);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// フレーズ画像を追加します。
        /// </summary>
        public async Task AddPhraseImageAsync(DPhraseImage image)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            context.DPhraseImages.Add(image);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// フレーズ画像を更新します。
        /// </summary>
        public async Task UpdatePhraseImageAsync(DPhraseImage image)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            context.DPhraseImages.Update(image);
            await context.SaveChangesAsync();
        }
    }
}
