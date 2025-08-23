using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class TestResultDetailRepository : BaseRepository<DTestResultDetail>
    {
        public TestResultDetailRepository(EngDbContext context) : base(context) { }

        public Task<List<DTestResultDetail>> GetByPhraseIdsAsync(IEnumerable<Guid> phraseIds)
        {
            var ids = phraseIds?.Distinct().ToArray() ?? Array.Empty<Guid>();
            return Set.Where(x => ids.Contains(x.PhraseId))
                      .ToListAsync();
        }

    }
}
