using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class OperationTypeRepository : BaseRepository<MOperationType>
    {
        public OperationTypeRepository(EngDbContext context) : base(context) { }

    }
}
