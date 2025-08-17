namespace PhrazorApp.Models
{
    /// <summary>フレーズ帳の候補（ドロップダウン用）</summary>
    public sealed class BulkPhraseBookOptionModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ItemCount { get; set; }
    }
}
