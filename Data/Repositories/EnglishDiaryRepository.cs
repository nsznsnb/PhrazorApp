using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class EnglishDiaryRepository : BaseRepository<DEnglishDiary>
    {
        public EnglishDiaryRepository(EngDbContext context) : base(context) { }

    }
}
