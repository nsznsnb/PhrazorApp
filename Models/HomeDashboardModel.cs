namespace PhrazorApp.Models
{
    /// <summary>チャート1点（Label, Value）</summary>
    public sealed record ChartPoint(string Label, double Value);


    /// <summary>ホームダッシュボード表示用ビューモデル</summary>
    public sealed class HomeDashboardModel
    {
        public int RegisteredPhraseCount { get; set; }
        public int LearnedPhraseCount { get; set; }

        public ProverbModel? TodayProverb { get; set; }

        // 折れ線・棒・ドーナツ用のシンプルデータ
        public IReadOnlyList<ChartPoint> DailyReviews { get; set; } = Array.Empty<ChartPoint>();
        public IReadOnlyList<ChartPoint> WeeklyNewPhrases { get; set; } = Array.Empty<ChartPoint>();
        public IReadOnlyList<ChartPoint> ReviewTypeBreakdown { get; set; } = Array.Empty<ChartPoint>();
        public IReadOnlyList<ChartPoint> TestAccuracyTimeline { get; set; } = Array.Empty<ChartPoint>();
    }
}
