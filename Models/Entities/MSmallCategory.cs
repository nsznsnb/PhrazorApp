using System;
using System.Collections.Generic;

namespace PhrazorApp.Models.Entities;

/// <summary>
/// 小分類マスタ
/// </summary>
public partial class MSmallCategory
{
    /// <summary>
    /// 大分類ID
    /// </summary>
    public Guid LargeId { get; set; }

    /// <summary>
    /// 小分類ID
    /// </summary>
    public Guid SmallId { get; set; }

    /// <summary>
    /// 小分類名
    /// </summary>
    public string SmallCategoryName { get; set; } = null!;

    /// <summary>
    /// ソート順
    /// </summary>
    public int SortOrder { get; set; }

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

    public virtual MLargeCategory Large { get; set; } = null!;

    public virtual ICollection<MPhraseCategory> MPhraseCategories { get; set; } = new List<MPhraseCategory>();
}
