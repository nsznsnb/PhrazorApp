using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class PhraseBookRepository : BaseRepository<MPhraseBook>
    {
        public PhraseBookRepository(EngDbContext context) : base(context) { }

        /// <summary>（ユーザーの）フレーズ帳を全件取得（グローバルフィルタ適用）</summary>
        public Task<List<MPhraseBook>> GetAllAsync()
            => Queryable(asNoTracking: true)
                .OrderBy(x => x.PhraseBookName)
                .ToListAsync();

        /// <summary>同名のフレーズ帳を 1 件取得（存在しなければ null）</summary>
        public Task<MPhraseBook?> FindByNameAsync(string name)
            => Queryable(asNoTracking: true)
                .FirstOrDefaultAsync(x => x.PhraseBookName == name);
    }
}
