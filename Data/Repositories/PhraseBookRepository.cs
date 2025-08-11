using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class PhraseBookRepository : BaseRepository<MPhraseBook>
    {
        public PhraseBookRepository(EngDbContext context) : base(context) { }

    }
}
