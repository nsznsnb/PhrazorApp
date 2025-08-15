using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class ReviewTypeRepository : BaseRepository<MReviewType>
    {
        public ReviewTypeRepository(EngDbContext context) : base(context) { }

        public Task<MReviewType?> GetByIdAsync(Guid id)
            => Set.FirstOrDefaultAsync(x => x.ReviewTypeId == id);
    }
}
