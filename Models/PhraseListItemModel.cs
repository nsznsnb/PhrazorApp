namespace PhrazorApp.Models
{
    public class PhraseListItemModel
    {
        public Guid Id { get; set; }
        public string Phrase { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public int ReviewCount { get; set; }
        public DateTime? CreatedAt { get; set; }                 // 一覧表示用
        public List<DropItemModel> SelectedDropItems { get; set; } = new(); // 表示用にチップ化

        // ▼ 追加：フレーズ帳名の一覧（フィルタ用）
        public List<string> PhraseBookNames { get; set; } = new();
    }
}
