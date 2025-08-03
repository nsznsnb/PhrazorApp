using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{

    public class PhraseRepository : BaseRepository<DPhrase>
    {

        /// <summary>
        /// すべてのフレーズを取得します。
        /// </summary>
        public async Task<List<DPhrase>> GetAllPhrasesAsync(EngDbContext context, string userId)
        {
            return await context.DPhrases
                .Where(x => x.UserId == userId)
                .Include(p => p.DPhraseImage)
                .Include(p => p.MPhraseGenres)
                .Include(p => p.DReviewLogs)
                .ToListAsync();
        }

        /// <summary>
        /// 指定されたフレーズIDのフレーズを取得します。
        /// </summary>
        public async Task<DPhrase?> GetPhraseByIdAsync(EngDbContext context, Guid? phraseId, string userId)
        {
            return await context.DPhrases
                .Where(x => x.UserId == userId)
                .Include(p => p.DPhraseImage)
                .Include(p => p.MPhraseGenres)
                .FirstOrDefaultAsync(p => p.PhraseId == phraseId);
        }



    }
}
