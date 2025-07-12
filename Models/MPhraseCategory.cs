using System;
using System.Collections.Generic;

namespace PhrazorApp.Models;

/// <summary>
/// フレーズ分類
/// </summary>
public partial class MPhraseCategory
{
    /// <summary>
    /// フレーズID
    /// </summary>
    public Guid PhraseId { get; set; }

    /// <summary>
    /// 小分類ID
    /// </summary>
    public Guid SmallId { get; set; }

    /// <summary>
    /// 大分類ID
    /// </summary>
    public Guid LargeId { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    public virtual MSmallCategory MSmallCategory { get; set; } = null!;

    public virtual DUserPhrase Phrase { get; set; } = null!;
}
