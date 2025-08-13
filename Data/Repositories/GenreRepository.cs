using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class GenreRepository : BaseRepository<MGenre>
    {
        public GenreRepository(EngDbContext context) : base(context) { }

        public Task<List<MGenre>> GetAllGenresAsync()
        {
            return _context.Set<MGenre>()
                .Include(x => x.MSubGenres)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }

        public Task<MGenre?> GetGenreByIdAsync(Guid genreId)
        {
            return _context.Set<MGenre>()
                .Include(x => x.MSubGenres)
                .FirstOrDefaultAsync(x => x.GenreId == genreId);
        }

    }
}
