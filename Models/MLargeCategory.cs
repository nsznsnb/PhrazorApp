using System;
using System.Collections.Generic;

namespace PhrazorApp.Models;

/// <summary>
/// 大分類マスタ
/// </summary>
public partial class MLargeCategory
{
    /// <summary>
    /// 大分類ID
    /// </summary>
    public Guid LargeId { get; set; }

    /// <summary>
    /// 大分類名
    /// </summary>
    public string LargeCategoryName { get; set; } = null!;

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

    public virtual ICollection<MSmallCategory> MSmallCategories { get; set; } = new List<MSmallCategory>();
}
