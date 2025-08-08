using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class PhraseRepository : BaseRepository<DPhrase>
    {
        public PhraseRepository(EngDbContext context) : base(context) { }

        public Task<List<DPhrase>> GetAllPhrasesAsync(string userId, CancellationToken ct = default)
        {
            return _context.Set<DPhrase>()
                .Where(x => x.UserId == userId)
                .Include(p => p.DPhraseImage)
                .Include(p => p.MPhraseGenres)
                .Include(p => p.DReviewLogs)
                .ToListAsync(ct);
        }

        public Task<DPhrase?> GetPhraseByIdAsync(Guid? phraseId, string userId, CancellationToken ct = default)
        {
            return _context.Set<DPhrase>()
                .Where(x => x.UserId == userId)
                .Include(p => p.DPhraseImage)
                .Include(p => p.MPhraseGenres)
                .FirstOrDefaultAsync(p => p.PhraseId == phraseId, ct);
        }

    }
}
