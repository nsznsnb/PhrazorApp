namespace PhrazorApp.Models
{
    /// <summary>成績マスタ（UI用）</summary>
    public sealed class GradeModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // 追加：並び順
        public int OrderNo { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
