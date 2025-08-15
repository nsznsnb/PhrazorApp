using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data.UnitOfWork;
using PhrazorApp.Models;
using System.Linq;

namespace PhrazorApp.Services
{
    /// <summary>
    /// ホームダッシュボード集計サービス
    /// - UoW 経由で必要データを集計
    /// - 「習得フレーズ」は暫定ルール：レビュー総数 >= 5 ＆ 正答率 >= 80%
    /// </summary>
    public sealed class HomeDashboardService
    {
        private readonly UnitOfWork _uow;
        private readonly UserService _user;

        public HomeDashboardService(UnitOfWork uow, UserService user)
        {
            _uow = uow;
            _user = user;
        }

        public async Task<ServiceResult<HomeDashboardModel>> GetAsync(DateTime? today = null)
        {
            var model = await _uow.ReadAsync(async repos =>
            {
                var userId = _user.GetUserId();
                var nowDate = (today ?? DateTime.UtcNow).Date;

                // --- 1) 登録/習得フレーズ数 ---
                var pQuery = repos.Phrases.Queryable(asNoTracking: true)
                                      .Where(p => p.UserId == userId);
                var registeredCount = await pQuery.CountAsync();

                var reviewAgg = await (
                    from rl in repos.ReviewLogs.Queryable(asNoTracking: true)
                    join p in pQuery on rl.PhraseId equals p.PhraseId
                    join d in repos.TestResultDetails.Queryable(asNoTracking: true)
                        on new { rl.TestId, rl.TestResultDetailNo }
                        equals new { d.TestId, d.TestResultDetailNo }
                    select new { rl.PhraseId, d.IsCorrect }
                ).ToListAsync();

                var learnedCount = reviewAgg
                    .GroupBy(x => x.PhraseId)
                    .Select(g => new { Total = g.Count(), Correct = g.Count(x => x.IsCorrect) })
                    .Count(x => x.Total >= 5 && (double)x.Correct / x.Total >= 0.8);

                // --- 2) 今日の格言（DayOfYearでローテーション） ---
                ProverbModel? todayProverb = null;
                var provCount = await repos.Proverbs.Queryable(asNoTracking: true).CountAsync();
                if (provCount > 0)
                {
                    var idx = (nowDate.DayOfYear - 1) % provCount;
                    todayProverb = await repos.Proverbs.Queryable(asNoTracking: true)
                        .OrderBy(x => x.ProverbId)
                        .Skip(idx).Take(1)
                        .Select(x => new ProverbModel
                        {
                            Id = x.ProverbId,
                            Text = x.ProverbText ?? string.Empty,
                            Meaning = x.Meaning ?? string.Empty,
                            Author = x.Author ?? string.Empty
                        })
                        .FirstAsync();
                }

                // --- 3) 学習記録（日次レビュー回数 直近14日・Bar）---
                var start = nowDate.AddDays(-13);
                var endExclusive = nowDate.AddDays(1);

                var rawDaily = await (
                    from rl in repos.ReviewLogs.Queryable(asNoTracking: true)
                    join p in pQuery on rl.PhraseId equals p.PhraseId
                    where rl.ReviewDate >= start && rl.ReviewDate < endExclusive
                    group rl by rl.ReviewDate.Date into g
                    select new { Day = g.Key, Count = g.Count() }
                ).ToListAsync();

                var daily = Enumerable.Range(0, 14)
                    .Select(i => start.AddDays(i))
                    .Select(d => new ChartPoint(d.ToString("MM/dd"),
                               rawDaily.FirstOrDefault(x => x.Day == d)?.Count ?? 0))
                    .ToList();

                // --- 4) 直近8週間の新規フレーズ（Bar）---
                var rawWeekly = await (
                    from p in pQuery
                    let weeksAgo = EF.Functions.DateDiffWeek(p.CreatedAt, nowDate)
                    where weeksAgo >= 0 && weeksAgo < 8
                    group p by weeksAgo into g
                    select new { WeeksAgo = g.Key, Count = g.Count() }
                ).ToListAsync();

                var weeklyList = Enumerable.Range(0, 8)
                    .Select(wa =>
                    {
                        var label = $"W-{7 - wa}"; // 古い→新しい
                        var c = rawWeekly.FirstOrDefault(x => x.WeeksAgo == wa)?.Count ?? 0;
                        return new ChartPoint(label, c);
                    })
                    .ToList();

                // --- 5) レビュー種別内訳（Donut）---
                var typeCounts = await (
                    from rl in repos.ReviewLogs.Queryable(asNoTracking: true)
                    join p in pQuery on rl.PhraseId equals p.PhraseId
                    group rl by rl.ReviewTypeId into g
                    select new { ReviewTypeId = g.Key, Count = g.Count() }
                ).ToListAsync();

                var typeNames = await repos.ReviewTypes.Queryable(asNoTracking: true)
                    .Select(rt => new { rt.ReviewTypeId, rt.ReviewTypeName })
                    .ToListAsync();

                var donut = (from tc in typeCounts
                             join tn in typeNames on tc.ReviewTypeId equals tn.ReviewTypeId into j
                             from tn in j.DefaultIfEmpty()
                             select new ChartPoint(tn?.ReviewTypeName ?? $"Type {tc.ReviewTypeId}", tc.Count))
                            .ToList();

                // --- 6) テスト正答率（Line, 直近10件）---
                var lastTests = await repos.TestResults.Queryable(asNoTracking: true)
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.TestDatetime)
                    .Take(10)
                    .Select(t => new { t.TestId, t.TestDatetime })
                    .ToListAsync();

                var testIds = lastTests.Select(x => x.TestId).ToList();
                var detailAgg = await repos.TestResultDetails.Queryable(asNoTracking: true)
                    .Where(d => testIds.Contains(d.TestId))
                    .GroupBy(d => d.TestId)
                    .Select(g => new { TestId = g.Key, Total = g.Count(), Correct = g.Count(x => x.IsCorrect) })
                    .ToListAsync();

                var line = lastTests
                    .OrderBy(x => x.TestDatetime) // 古い→新しい
                    .Select(t =>
                    {
                        var a = detailAgg.FirstOrDefault(x => x.TestId == t.TestId);
                        double acc = (a is null || a.Total == 0) ? 0 : (100.0 * a.Correct / a.Total);
                        return new ChartPoint(t.TestDatetime.ToString("MM/dd HH:mm"), Math.Round(acc, 1));
                    })
                    .ToList();

                return new HomeDashboardModel
                {
                    RegisteredPhraseCount = registeredCount,
                    LearnedPhraseCount = learnedCount,
                    TodayProverb = todayProverb,
                    DailyReviews = daily,
                    WeeklyNewPhrases = weeklyList,
                    ReviewTypeBreakdown = donut,
                    TestAccuracyTimeline = line
                };
            });

            return ServiceResult.Success(model);
        }

    }
}
