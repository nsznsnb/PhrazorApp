using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class DiaryTagRepository : BaseRepository<MDiaryTag>
    {
        public DiaryTagRepository(EngDbContext context) : base(context) { }

        public Task<List<MDiaryTag>> GetAllAsync()
        {
            return _context.Set<MDiaryTag>()
                      .AsNoTracking()
                      .OrderBy(t => t.TagName)
                      .ToListAsync();
        }

    }
}
