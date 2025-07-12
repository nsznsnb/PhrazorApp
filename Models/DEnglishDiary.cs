using System;
using System.Collections.Generic;

namespace PhrazorApp.Models;

/// <summary>
/// 英語日記
/// </summary>
public partial class DEnglishDiary
{
    /// <summary>
    /// 英語日記ID
    /// </summary>
    public Guid DiaryId { get; set; }

    /// <summary>
    /// タイトル
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// ユーザーId
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<DEnglishDiaryTag> DEnglishDiaryTags { get; set; } = new List<DEnglishDiaryTag>();
}
