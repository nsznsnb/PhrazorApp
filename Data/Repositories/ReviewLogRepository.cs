using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class ReviewLogRepository : BaseRepository<DReviewLog>
    {
        public ReviewLogRepository(EngDbContext context) : base(context) { }

    }
}
