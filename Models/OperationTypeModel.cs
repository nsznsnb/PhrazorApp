namespace PhrazorApp.Models
{
    /// <summary>操作種別マスタ（UI用）</summary>
    public sealed class OperationTypeModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int? Limit { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
