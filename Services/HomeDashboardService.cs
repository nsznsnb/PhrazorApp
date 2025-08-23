using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data.UnitOfWork;
using PhrazorApp.Models;

namespace PhrazorApp.Services
{
    /// <summary>
    /// ホームダッシュボード集計サービス
    /// - 「習得フレーズ」は暫定ルール：レビュー総数 >= 5 ＆ 正答率 >= 80%
    /// - 直近1か月の新規登録は日単位、実データのみ（全ゼロならカードは空状態）
    /// - 成績構成比（S/A/B/D）は全テスト累計の割合
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
            var model = await _uow.ReadAsync(async (UowRepos repos) =>
            {
                var userId = _user.GetUserId();
                var nowDate = (today ?? DateTime.UtcNow).Date;
                var endExclusive = nowDate.AddDays(1);


                // --- 1) 登録/習得フレーズ数 ---
                var pQuery = repos.Phrases.Queryable(asNoTracking: true)
                                          .Where(p => p.UserId == userId);

                var registeredCount = await pQuery.CountAsync();


                const int MIN_TOTAL = 2;   // 最低試行回数
                const int MIN_RATE = 60;  // 合格率(％)

                var learnedCount = await (
                    from rl in repos.ReviewLogs.Queryable(asNoTracking: true)
                    join p in pQuery on rl.PhraseId equals p.PhraseId
                    join d in repos.TestResultDetails.Queryable(asNoTracking: true)
                        on new { rl.TestId, rl.TestResultDetailNo }
                        equals new { d.TestId, d.TestResultDetailNo }
                    group d by rl.PhraseId into g
                    select new
                    {
                        PhraseId = g.Key,
                        Total = g.Count(),
                        Correct = g.Count(x => x.IsCorrect == true) // bool? 対応
                    }
                )
                .Where(x => x.Total >= MIN_TOTAL && (100 * x.Correct) >= (MIN_RATE * x.Total))
                .CountAsync();

                // --- 2) 今日の格言 ---
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

                // --- 3) 学習記録（日次レビュー回数 直近14日：連続学習日数に使用） ---
                var start14 = nowDate.AddDays(-13);
                var rawDaily = await (
                    from rl in repos.ReviewLogs.Queryable(asNoTracking: true)
                    join p in pQuery on rl.PhraseId equals p.PhraseId
                    where rl.ReviewDate >= start14 && rl.ReviewDate < endExclusive
                    group rl by rl.ReviewDate.Date into g
                    select new { Day = g.Key, Count = g.Count() }
                ).ToListAsync();

                var daily = Enumerable.Range(0, 14)
                    .Select(i => start14.AddDays(i))
                    .Select(d => new ChartPoint(d.ToString("M/d"),
                               rawDaily.FirstOrDefault(x => x.Day == d)?.Count ?? 0))
                    .ToList();

                // --- 4) 直近8週間の新規フレーズ（KPI：今週/先週比に利用） ---
                var rawWeekly = await (
                    from p in pQuery
                    let daysAgo = EF.Functions.DateDiffDay(p.CreatedAt, nowDate) // int? が返る
                    where daysAgo >= 0 && daysAgo < 56
                    group p by (daysAgo / 7) into g                      // 0=今週, 1=先週, ...
                    select new { WeeksAgo = g.Key, Count = g.Count() }
                ).ToListAsync();

                var weeklyList = Enumerable.Range(0, 8)
                    .Select(wa =>
                    {
                        var c = rawWeekly.FirstOrDefault(x => x.WeeksAgo == wa)?.Count ?? 0;
                        return new ChartPoint($"W-{7 - wa}", c); // 古い→新しいで表示
                    })
                    .ToList();

                // --- 5) 直近1か月の新規フレーズ（日次／実データのみ） ---
                var monthStart = nowDate.AddDays(-29); // 今日含む30日間
                var rawMonth = await pQuery
                    .Where(p => p.CreatedAt >= monthStart && p.CreatedAt < endExclusive)
                    .GroupBy(p => p.CreatedAt.Date)
                    .Select(g => new { Day = g.Key, Count = g.Count() })
                    .ToListAsync();

                var lastMonthNew = Enumerable.Range(0, 30)
                    .Select(i => monthStart.AddDays(i))
                    .Select(d => new ChartPoint(d.ToString("M/d"),
                               rawMonth.FirstOrDefault(x => x.Day == d)?.Count ?? 0))
                    .ToList();

                // --- 6) 成績構成比（S/A/B/D の件数） ---
                var gradeCounts = await (
                    from t in repos.TestResults.Queryable(asNoTracking: true)
                    join g in repos.Grades.Queryable(asNoTracking: true) on t.GradeId equals g.GradeId
                    where t.UserId == userId
                    group g by g.GradeName into gg
                    select new { Grade = gg.Key, Count = gg.Count() }
                ).ToListAsync();

                // 並び順を固定（S→A→B→D）。存在しない成績は 0 件にする
                string[] order = new[] { "S", "A", "B", "D" };
                var gradeDistribution = order
                    .Select(sym => new ChartPoint(sym, gradeCounts.FirstOrDefault(x => x.Grade == sym)?.Count ?? 0))
                    .ToList();

                return new HomeDashboardModel
                {
                    RegisteredPhraseCount = registeredCount,
                    LearnedPhraseCount = learnedCount,
                    TodayProverb = todayProverb,
                    DailyReviews = daily,               // 連続学習用
                    WeeklyNewPhrases = weeklyList,      // KPI用
                    LastMonthNewPhrases = lastMonthNew, // 左下
                    GradeDistribution = gradeDistribution // 右下（Donut）
                };
            });

            return ServiceResult.Success(model);
        }
    }
}
