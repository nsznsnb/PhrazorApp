using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class PhraseBookItemRepository : BaseRepository<MPhraseBookItem>
    {
        public PhraseBookItemRepository(EngDbContext context) : base(context) { }

    }
}
