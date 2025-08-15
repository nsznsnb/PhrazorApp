namespace PhrazorApp.Models
{
    /// <summary>成績マスタ（UI用）</summary>
    public sealed class GradeModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
