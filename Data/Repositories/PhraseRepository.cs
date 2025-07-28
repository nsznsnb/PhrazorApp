using Humanizer.Localisation;
using Microsoft.EntityFrameworkCore;
using PhrazorApp.Commons;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{

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
                .Where(x => x.UserId == Common.GetUserId())
                .Include(p => p.DPhraseImage)
                .Include(p => p.MPhraseGenres)
                .ToListAsync();
        }

        /// <summary>
        /// 指定されたフレーズIDのフレーズを取得します。
        /// </summary>
        public async Task<DPhrase?> GetPhraseByIdAsync(Guid? phraseId)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.DPhrases
                .Where(x => x.UserId == Common.GetUserId())
                .Include(p => p.DPhraseImage)
                .Include(p => p.MPhraseGenres)
                .FirstOrDefaultAsync(p => p.PhraseId == phraseId);
        }

        /// <summary>
        /// 新しいフレーズを追加します。
        /// </summary>
        public void CreatePhrase(EngDbContext context, DPhrase phrase)
        {
            var now = DateTime.UtcNow;
            phrase.UserId = Common.GetUserId();
            phrase.CreatedAt = now;
            phrase.UpdatedAt = now;

            context.DPhrases.Add(phrase);
        }

        /// <summary>
        /// フレーズ情報を更新します。
        /// </summary>
        public void UpdatePhrase(EngDbContext context, DPhrase phrase)
        {
            var now = DateTime.UtcNow;
            phrase.UserId = Common.GetUserId();
            phrase.UpdatedAt = now;
            context.DPhrases.Update(phrase);
        }

        /// <summary>
        /// フレーズを削除します。
        /// </summary>
        public void DeletePhrase(EngDbContext context, DPhrase phrase)
        {
            context.DPhrases.Remove(phrase);
        }

        /// <summary>
        /// フレーズジャンルを追加します。
        /// </summary>
        public void CreatePhraseGenreRange(EngDbContext context, IEnumerable<MPhraseGenre> phraseGenres)
        {
            var now = DateTime.UtcNow;
            var userId = Common.GetUserId();
            foreach (var phraseGenre in phraseGenres)
            {
                phraseGenre.CreatedAt = now;
                phraseGenre.UpdatedAt = now;
            }

            context.MPhraseGenres.AddRange(phraseGenres);
        }

        /// <summary>
        /// フレーズジャンルを削除します。
        /// </summary>
        public void DeletePhraseGenreRange(EngDbContext context, IEnumerable<MPhraseGenre> phraseGenres)
        {
            context.MPhraseGenres.RemoveRange(phraseGenres);
        }

        /// <summary>
        /// フレーズ画像を追加します。
        /// </summary>
        public void CreatePhraseImage(EngDbContext context, DPhraseImage image)
        {
            var now = DateTime.UtcNow;
            image.CreatedAt = now;
            image.UpdatedAt = now;
            context.DPhraseImages.Add(image);
        }

        /// <summary>
        /// フレーズ画像を更新します。
        /// </summary>
        public void UpdatePhraseImage(EngDbContext context, DPhraseImage image)
        {
            var now = DateTime.UtcNow;
            image.CreatedAt = now;
            image.UpdatedAt = now;

            context.DPhraseImages.Update(image);
        }

        public void DeletePhraseImage(EngDbContext context, DPhraseImage image)
        {
            context.DPhraseImages.Remove(image);
        }
    }
}
