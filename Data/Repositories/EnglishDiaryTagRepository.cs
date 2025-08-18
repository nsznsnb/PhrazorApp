using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class EnglishDiaryTagRepository : BaseRepository<DEnglishDiaryTag>
    {
        public EnglishDiaryTagRepository(EngDbContext context) : base(context) { }

    }
}
