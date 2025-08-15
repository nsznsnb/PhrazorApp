using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;
using System.Collections.Generic;

namespace PhrazorApp.Data.Repositories
{
    public class PhraseGenreRepository : BaseRepository<MPhraseGenre>
    {
        public PhraseGenreRepository(EngDbContext context) : base(context) { }

        public Task<int> CountByGenreIdAsync(Guid genreId)
            => Set.Where(x => x.GenreId == genreId).CountAsync();

        public Task<int> CountBySubGenreIdsAsync(IEnumerable<Guid> subGenreIds)
        {
            var ids = (subGenreIds ?? Enumerable.Empty<Guid>()).ToArray();
            if (ids.Length == 0) return Task.FromResult(0);
            return Set.Where(x => ids.Contains(x.SubGenreId)).CountAsync();
        }
    }
}
