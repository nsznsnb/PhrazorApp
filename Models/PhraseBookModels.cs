namespace PhrazorApp.Models
{
    // セレクト用 + 件数
    public class PhraseBookListItemModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    // フレーズ帳の中身の1行
    public class PhraseBookItemModel
    {
        public Guid Id { get; set; }            // DPhrase.PhraseId
        public string? English { get; set; }
        public string? Japanese { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    // 追加候補（Autocomplete）
    public class PhraseSuggestionModel
    {
        public Guid Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public override string ToString() => Label;
    }
}
