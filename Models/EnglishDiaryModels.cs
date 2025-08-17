using System.ComponentModel.DataAnnotations;
using Heron.MudCalendar; // CalendarItem を継承

namespace PhrazorApp.Models
{
    /// <summary>英語日記（編集/保存/表示 共通の画面用モデル）</summary>
    public class EnglishDiaryModel
    {
        [Display(Name = "英語日記Id")]
        public Guid Id { get; set; }

        [Display(Name = "タイトル")]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "内容（日記本文）")]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "補足（添削を正確にするための情報）")]
        [MaxLength(1000)]
        public string? Note { get; set; }

        [Display(Name = "解説（任意）")]
        [MaxLength(1000)]
        public string? Explanation { get; set; }

        [Display(Name = "添削結果")]
        [MaxLength(1000)]
        public string? Correction { get; set; }

        /// <summary>選択されたタグID郡（DiaryTagSelectorContainer と双方向バインド）</summary>
        public List<Guid> TagIds { get; set; } = new();

        [Display(Name = "作成日時(UTC)")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "更新日時(UTC)")]
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>カレンダー表示用アイテム（Heron.MudCalendar）</summary>
    public sealed class DiaryCalendarItem : CalendarItem
    {
        public Guid DiaryId { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
