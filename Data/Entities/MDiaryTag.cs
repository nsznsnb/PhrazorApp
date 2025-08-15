using System;
using System.Collections.Generic;

namespace PhrazorApp.Data.Entities;

/// <summary>
/// 日記タグ
/// </summary>
public partial class MDiaryTag
{
    /// <summary>
    /// 日記タグID
    /// </summary>
    public Guid TagId { get; set; }

    /// <summary>
    /// タグ名
    /// </summary>
    public string? TagName { get; set; }

    /// <summary>
    /// ユーザーID
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
