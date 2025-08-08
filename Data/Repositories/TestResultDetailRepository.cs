using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class TestResultDetailRepository : BaseRepository<DTestResultDetail>
    {
        public TestResultDetailRepository(EngDbContext context) : base(context) { }

    }
}
