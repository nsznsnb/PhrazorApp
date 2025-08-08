using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class ProverbRepository : BaseRepository<MProverb>
    {
        public ProverbRepository(EngDbContext context) : base(context) { }

    }
}
