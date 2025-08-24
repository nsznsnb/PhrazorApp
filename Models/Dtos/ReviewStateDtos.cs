namespace PhrazorApp.Models.Dtos
{


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
