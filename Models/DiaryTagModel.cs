using System.ComponentModel.DataAnnotations;

namespace PhrazorApp.Models
{
    /// <summary>日記タグ（画面モデル / DTO）</summary>
    public sealed class DiaryTagModel
    {
        public Guid Id { get; set; }
        [Display(Name = "タグ名")]
        public string? Name { get; set; }

        // 一覧での将来拡張に備えて保持（未使用なら表示しない）
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
