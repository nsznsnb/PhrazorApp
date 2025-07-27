using System;
using System.Collections.Generic;

namespace PhrazorApp.Data.Entities;

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
    public string Title { get; set; } = null!;

    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// 補足
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// 添削結果
    /// </summary>
    public string? Correction { get; set; }

    /// <summary>
    /// 解説
    /// </summary>
    public string? Explanation { get; set; }

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
