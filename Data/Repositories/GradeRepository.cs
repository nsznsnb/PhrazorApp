using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class GradeRepository : BaseRepository<MGrade>
    {
        public GradeRepository(EngDbContext context) : base(context) { }

    }
}
