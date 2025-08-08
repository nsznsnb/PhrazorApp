using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class TestResultRepository : BaseRepository<DTestResult>
    {
        public TestResultRepository(EngDbContext context) : base(context) { }

    }
}
