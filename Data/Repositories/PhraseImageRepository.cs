using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class PhraseImageRepository : BaseRepository<DPhraseImage>
    {
        public PhraseImageRepository(EngDbContext context) : base(context) { }

    }
}
