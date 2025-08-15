using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class GradeRepository : BaseRepository<MGrade>
    {
        public GradeRepository(EngDbContext context) : base(context) { }

        public async Task<MGrade?> GetByIdAsync(Guid id)
           => await Set.FirstOrDefaultAsync(x => x.GradeId == id);
    }
}
