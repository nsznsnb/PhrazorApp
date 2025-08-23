using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class PhraseBookItemRepository : BaseRepository<MPhraseBookItem>
    {
        public PhraseBookItemRepository(EngDbContext context) : base(context) { }

        public Task<List<MPhraseBookItem>> GetByPhraseIdsAsync(IEnumerable<Guid> phraseIds, CancellationToken ct = default)
        {
            var ids = phraseIds?.Distinct().ToArray() ?? Array.Empty<Guid>();
            return Set.Where(x => ids.Contains(x.PhraseId))
                      .ToListAsync(ct);
        }


    }
}
