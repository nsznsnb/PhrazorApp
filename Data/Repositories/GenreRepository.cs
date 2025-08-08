using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class GenreRepository : BaseRepository<MGenre>
    {
        public GenreRepository(EngDbContext context) : base(context) { }

        public Task<List<MGenre>> GetAllGenresAsync(string userId, CancellationToken ct = default)
        {
            return _context.Set<MGenre>()
                .Where(x => x.UserId == userId)
                .Include(x => x.MSubGenres)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(ct);
        }

        public Task<MGenre?> GetGenreByIdAsync(Guid genreId, string userId, CancellationToken ct = default)
        {
            return _context.Set<MGenre>()
                .Include(x => x.MSubGenres)
                .FirstOrDefaultAsync(x => x.GenreId == genreId && x.UserId == userId, ct);
        }

    }
}
