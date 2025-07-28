using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public interface IPhraseRepository
    {
        Task<List<DPhrase>> GetAllPhrasesAsync();
        Task<DPhrase?> GetPhraseByIdAsync(Guid phraseId);
        Task CreatePhraseAsync(DPhrase phrase);
        Task UpdatePhraseAsync(DPhrase phrase);
        Task DeletePhraseAsync(DPhrase phrase);
        Task CreatePhraseGenreAsync(IEnumerable<MPhraseGenre> phraseGenres);
        Task DeletePhraseGenresAsync(IEnumerable<MPhraseGenre> phraseGenres);
        Task CreatePhraseImageAsync(DPhraseImage image);
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
                .Include(p => p.MPhraseGenres)
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
                .Include(p => p.MPhraseGenres)
                .FirstOrDefaultAsync(p => p.PhraseId == phraseId);
        }

        /// <summary>
        /// 新しいフレーズを追加します。
        /// </summary>
        public async Task CreatePhraseAsync(DPhrase phrase)
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
        /// フレーズジャンルを追加します。
        /// </summary>
        public async Task CreatePhraseGenreAsync(IEnumerable<MPhraseGenre> phraseGenres)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            context.MPhraseGenres.AddRange(phraseGenres);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// フレーズジャンルを削除します。
        /// </summary>
        public async Task DeletePhraseGenresAsync(IEnumerable<MPhraseGenre> phraseGenres)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            context.MPhraseGenres.RemoveRange(phraseGenres);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// フレーズ画像を追加します。
        /// </summary>
        public async Task CreatePhraseImageAsync(DPhraseImage image)
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
