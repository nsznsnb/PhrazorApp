using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;
using PhrazorApp.Models;
using PhrazorApp.Models.Mappings;

namespace PhrazorApp.Data.Repositories
{
    public class ProverbRepository : BaseRepository<MProverb>
    {
        public ProverbRepository(EngDbContext context) : base(context) { }

        public Task<List<ProverbModel>> GetListProjectedAsync()
            => _context.Set<MProverb>()
                       .Select(ProverbModelMapper.ListProjection)
                       .OrderByDescending(x => x.CreatedAt ?? DateTime.MinValue)
                       .ToListAsync();

        public Task<MProverb?> GetByIdAsync(Guid id)
            => _context.Set<MProverb>().FirstOrDefaultAsync(x => x.ProverbId == id);

        public Task<MProverb?> GetByTextAuthorAsync(string text, string? author)
            => _context.Set<MProverb>()
                       .FirstOrDefaultAsync(p => p.ProverbText == text && (p.Author ?? "") == (author ?? ""));

        /// <summary>
        /// Text + Author をキーにした Upsert。既存は更新、無ければ追加。
        /// </summary>
        public async Task UpsertRangeByTextAuthorAsync(IEnumerable<MProverb> incoming)
        {
            var set = _context.Set<MProverb>();
            // まとめて既存候補を引く（Text,Author）で照合
            var texts = incoming.Select(i => i.ProverbText).Where(t => !string.IsNullOrWhiteSpace(t)).Distinct().ToArray();
            var authors = incoming.Select(i => i.Author ?? "").Distinct().ToArray();

            // 既存をできるだけまとめて取得（Text が多いときは分割も可）
            var existing = await set
                .Where(p => texts.Contains(p.ProverbText) && authors.Contains(p.Author ?? ""))
                .ToListAsync();

            foreach (var it in incoming)
            {
                var found = existing.FirstOrDefault(e => e.ProverbText == it.ProverbText &&
                                                         (e.Author ?? "") == (it.Author ?? ""));
                if (found is null)
                {
                    await AddAsync(it); // BaseRepository が CreatedAt/UpdatedAt を Stamp
                }
                else
                {
                    found.Meaning = it.Meaning;
                    found.Author = it.Author;
                    await UpdateAsync(found);
                }
            }
        }
    }
}
