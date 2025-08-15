using System;
using System.Collections.Generic;

namespace PhrazorApp.Data.Entities;

/// <summary>
/// 復習種別マスタ
/// </summary>
public partial class MReviewType
{
    /// <summary>
    /// 復習種別ID
    /// </summary>
    public Guid ReviewTypeId { get; set; }

    /// <summary>
    /// 復習種別名
    /// </summary>
    public string ReviewTypeName { get; set; } = null!;

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<DReviewLog> DReviewLogs { get; set; } = new List<DReviewLog>();
}
