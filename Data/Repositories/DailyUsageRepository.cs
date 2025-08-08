using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class DailyUsageRepository : BaseRepository<DDailyUsage>
    {
        public DailyUsageRepository(EngDbContext context) : base(context) { }

    }
}
