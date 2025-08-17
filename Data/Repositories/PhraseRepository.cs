using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Entities;
using PhrazorApp.Models;
using PhrazorApp.Models.Mappings;

namespace PhrazorApp.Data.Repositories
{
    public class PhraseRepository : BaseRepository<DPhrase>
    {
        public PhraseRepository(EngDbContext context) : base(context) { }

        public Task<DPhrase?> GetPhraseByIdAsync(Guid? phraseId)
        {
            return _context.Set<DPhrase>()
                .Include(p => p.DPhraseImage)
                .Include(p => p.MPhraseGenres)
                    .ThenInclude(pg => pg.MSubGenre) // ★ 追加
                .FirstOrDefaultAsync(p => p.PhraseId == phraseId);
        }

        public Task<List<DPhrase>> GetAllPhrasesAsync()
        {
            return _context.Set<DPhrase>()
                .Include(p => p.DPhraseImage)
                .Include(p => p.MPhraseGenres)
                    .ThenInclude(pg => pg.MSubGenre) // ★ 追加
                .Include(p => p.DReviewLogs)
                .AsSplitQuery()
                .ToListAsync();
        }

        public async Task<List<DPhrase>> GetByPhrasesIdsAsync(IEnumerable<Guid> ids)
        {
            if (ids is null) throw new ArgumentNullException(nameof(ids));

            var idArray = ids.Distinct().ToArray();
            if (idArray.Length == 0) return new List<DPhrase>();

            return await _context.Set<DPhrase>()
                .Where(p => idArray.Contains(p.PhraseId))
                .Include(p => p.DPhraseImage)
                .Include(p => p.MPhraseGenres)
                .AsSplitQuery()
                .ToListAsync();
        }

        public Task<List<PhraseListItemModel>> GetListProjectedAsync(string userId)
        {
            return _context.Set<DPhrase>()
                .Where(p => p.UserId == userId)
                .Select(PhraseModelMapper.ListProjection)     // ★ EFでDTOに投影
                .OrderByDescending(x => x.CreatedAt ?? DateTime.MinValue)
                .ToListAsync();
        }


        public Task<List<PhraseListItemModel>> GetListByFilterProjectedAsync(string userId, TestFilterModel f)
        {
            var q = _context.Set<DPhrase>().Where(p => p.UserId == userId);

            // ★フレーズ帳：複数対応
            if (f.PhraseBookIds is { Count: > 0 })
            {
                var ids = f.PhraseBookIds.ToArray();
                q = q.Where(p => _context.Set<MPhraseBookItem>()
                             .Any(i => ids.Contains(i.PhraseBookId) && i.PhraseId == p.PhraseId));
            }

            //if (f.GenreId is Guid gId)
            //    q = q.Where(p => p.MPhraseGenres.Any(pg => pg.GenreId == gId));
            //if (f.SubGenreId is Guid sgId)
            //    q = q.Where(p => p.MPhraseGenres.Any(pg => pg.SubGenreId == sgId));

            if (f.DatePreset is DateRangePreset.Today or DateRangePreset.Yesterday or DateRangePreset.Last7Days or DateRangePreset.Last30Days)
            {
                var today = DateTime.Today;
                var (from, to) = f.DatePreset switch
                {
                    DateRangePreset.Today => (today, today.AddDays(1)),
                    DateRangePreset.Yesterday => (today.AddDays(-1), today),
                    DateRangePreset.Last7Days => (today.AddDays(-7), today.AddDays(1)),
                    DateRangePreset.Last30Days => (today.AddDays(-30), today.AddDays(1)),
                    _ => (DateTime.MinValue, DateTime.MaxValue)
                };
                q = q.Where(p => p.CreatedAt >= from && p.CreatedAt < to);
            }
            else if (f.DatePreset == DateRangePreset.Custom && f.DateFrom.HasValue && f.DateTo.HasValue)
            {
                var from = f.DateFrom.Value.Date;
                var toEx = f.DateTo.Value.Date.AddDays(1);
                q = q.Where(p => p.CreatedAt >= from && p.CreatedAt < toEx);
            }

            if (f.UntestedOnly)
            {
                q = q.Where(p => !_context.Set<DTestResultDetail>()
                            .Any(d => d.PhraseId == p.PhraseId && d.Test!.UserId == userId));
            }

            q = q.OrderByDescending(p => p.CreatedAt);

            var projected = q.Select(PhraseModelMapper.ListProjection);
            if (f.Limit > 0) projected = projected.Take(f.Limit);

            return projected.ToListAsync();
        }



    }
}
