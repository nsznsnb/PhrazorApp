using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class PhraseGenreRepository : BaseRepository<MPhraseGenre>
    {
        public PhraseGenreRepository(EngDbContext context) : base(context) { }

    }
}
