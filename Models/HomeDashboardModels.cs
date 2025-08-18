// PhrazorApp.Models.HomeDashboardModel.cs （一例）
using PhrazorApp.Models;

public sealed class HomeDashboardModel
{
    public int RegisteredPhraseCount { get; set; }
    public int LearnedPhraseCount { get; set; }
    public ProverbModel? TodayProverb { get; set; }

    public List<ChartPoint> DailyReviews { get; set; } = new();          // KPI用
    public List<ChartPoint> WeeklyNewPhrases { get; set; } = new();      // KPI用
    public List<ChartPoint> LastMonthNewPhrases { get; set; } = new();   // ★新規
    public List<ChartPoint> TestAccuracyTimeline { get; set; } = new();
}

public sealed class ChartPoint
{
    public ChartPoint(string label, double value) { Label = label; Value = value; }
    public string Label { get; set; }
    public double Value { get; set; }
}
