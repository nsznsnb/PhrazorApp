using System;
using System.Collections.Generic;

namespace PhrazorApp.Data.Entities;

/// <summary>
/// 英語日記タグ
/// </summary>
public partial class DEnglishDiaryTag
{
    /// <summary>
    /// 英語日記ID
    /// </summary>
    public Guid DiaryId { get; set; }

    /// <summary>
    /// 日記タグID
    /// </summary>
    public Guid DiaryTagId { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual DEnglishDiary Diary { get; set; } = null!;

    public virtual MDiaryTag DiaryTag { get; set; } = null!;
}
