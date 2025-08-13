using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class PhraseRepository : BaseRepository<DPhrase>
    {
        public PhraseRepository(EngDbContext context) : base(context) { }

        public Task<List<DPhrase>> GetAllPhrasesAsync()
        {
            return _context.Set<DPhrase>()
                .Include(p => p.DPhraseImage)
                .Include(p => p.MPhraseGenres)
                .Include(p => p.DReviewLogs)
                .ToListAsync();
        }

        public Task<DPhrase?> GetPhraseByIdAsync(Guid? phraseId)
        {
            return _context.Set<DPhrase>()
                .Include(p => p.DPhraseImage)
                .Include(p => p.MPhraseGenres)
                .FirstOrDefaultAsync(p => p.PhraseId == phraseId);
        }

        public async Task<List<DPhrase>> GetByPhrasesIdsAsync(IEnumerable<Guid> ids)
        {
            if (ids is null) throw new ArgumentNullException(nameof(ids));

            var idArray = ids.Distinct().ToArray();
            if (idArray.Length == 0) return new List<DPhrase>();

            return await _context.Set<DPhrase>()
                .Where(p => idArray.Contains(p.PhraseId))
                .Include(p => p.DPhraseImage)
                .Include(p => p.MPhraseGenres)
                .AsSplitQuery()
                .ToListAsync();
        }

    }
}
