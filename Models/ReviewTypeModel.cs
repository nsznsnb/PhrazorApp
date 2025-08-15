namespace PhrazorApp.Models
{
    /// <summary>復習種別マスタ（UI用）</summary>
    public sealed class ReviewTypeModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
