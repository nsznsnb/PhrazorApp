namespace PhrazorApp.Models.Dtos
{
    public static class ReviewStorage
    {
        public const string ReviewStateV1 = "review.state.v1";
        public const string ReviewResultV1 = "test.result.v1";
        public const string ShufflePrefV1 = "review.pref.shuffle.v1";
        public const string ReviewResultSigV1 = "test.result.sig.v1";
        public const string ReviewRetestFlagV1 = "review.retest.flag.v1";
    }

    // 出題カード
    public sealed class ReviewCardDto
    {
        public Guid PhraseId { get; set; }
        public string Front { get; set; } = "";
        public string Back { get; set; } = "";
    }

    // 出題状態
    public sealed class ReviewState
    {
        public List<ReviewCardDto> Items { get; set; } = new();
        public int Index { get; set; }
        public bool IsBack { get; set; }
        public bool Shuffled { get; set; }
        public DateTime SavedAtUtc { get; set; }
    }

    // テスト結果
    public sealed class TestResultRowDto
    {
        public string Front { get; set; } = "";
        public string Back { get; set; } = "";
        public bool Correct { get; set; }
    }
    public sealed class TestResultState
    {
        public List<TestResultRowDto> Rows { get; set; } = new();
        public DateTime SavedAtUtc { get; set; }
    }
}
