using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class PhraseRepository : BaseRepository<DPhrase>
    {
        public PhraseRepository(EngDbContext context) : base(context) { }

        public Task<List<DPhrase>> GetAllPhrasesAsync(string userId, CancellationToken ct)
        {
            return _context.Set<DPhrase>()
                .Where(x => x.UserId == userId)
                .Include(p => p.DPhraseImage)
                .Include(p => p.MPhraseGenres)
                .Include(p => p.DReviewLogs)
                .ToListAsync(ct);
        }

        public Task<DPhrase?> GetPhraseByIdAsync(Guid? phraseId, string userId, CancellationToken ct)
        {
            return _context.Set<DPhrase>()
                .Where(x => x.UserId == userId)
                .Include(p => p.DPhraseImage)
                .Include(p => p.MPhraseGenres)
                .FirstOrDefaultAsync(p => p.PhraseId == phraseId, ct);
        }

        public async Task<List<DPhrase>> GetByPhrasesIdsAsync(IEnumerable<Guid> ids, string userId, CancellationToken ct)
        {
            if (ids is null) throw new ArgumentNullException(nameof(ids));

            var idArray = ids.Distinct().ToArray();
            if (idArray.Length == 0) return new List<DPhrase>();

            return await _context.Set<DPhrase>()
                .Where(p => p.UserId == userId && idArray.Contains(p.PhraseId))
                .Include(p => p.DPhraseImage)
                .Include(p => p.MPhraseGenres)
                .AsSplitQuery()
                .ToListAsync(ct);
        }

    }
}
