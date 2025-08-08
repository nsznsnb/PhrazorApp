using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class SubGenreRepository : BaseRepository<MSubGenre>
    {
        public SubGenreRepository(EngDbContext context) : base(context) { }

    }
}
