using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;
using PhrazorApp.Models;
using PhrazorApp.Models.Mappings;

namespace PhrazorApp.Data.Repositories
{
    public class PhraseRepository : BaseRepository<DPhrase>
    {
        public PhraseRepository(EngDbContext context) : base(context) { }

        public Task<DPhrase?> GetPhraseByIdAsync(Guid? phraseId)
        {
            return _context.Set<DPhrase>()
                .Include(p => p.DPhraseImage)
                .Include(p => p.MPhraseGenres)
                    .ThenInclude(pg => pg.MSubGenre) // š ’Ç‰Á
                .FirstOrDefaultAsync(p => p.PhraseId == phraseId);
        }

        public Task<List<DPhrase>> GetAllPhrasesAsync()
        {
            return _context.Set<DPhrase>()
                .Include(p => p.DPhraseImage)
                .Include(p => p.MPhraseGenres)
                    .ThenInclude(pg => pg.MSubGenre) // š ’Ç‰Á
                .Include(p => p.DReviewLogs)
                .AsSplitQuery()
                .ToListAsync();
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

        public Task<List<PhraseListItemModel>> GetListProjectedAsync(string userId)
        {
            return _context.Set<DPhrase>()
                .Where(p => p.UserId == userId)
                .Select(PhraseModelMapper.ListProjection)     // š EF‚ÅDTO‚É“Š‰e
                .OrderByDescending(x => x.CreatedAt ?? DateTime.MinValue)
                .ToListAsync();
        }

    }
}
