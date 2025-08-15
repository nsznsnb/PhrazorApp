using System;
using System.Collections.Generic;

namespace PhrazorApp.Data.Entities;

/// <summary>
/// 操作種別マスタ
/// </summary>
public partial class MOperationType
{
    /// <summary>
    /// 操作種別ID
    /// </summary>
    public Guid OperationTypeId { get; set; }

    /// <summary>
    /// 操作種別コード
    /// </summary>
    public string OperationTypeCode { get; set; } = null!;

    /// <summary>
    /// 操作種別名
    /// </summary>
    public string OperationTypeName { get; set; } = null!;

    /// <summary>
    /// 操作回数上限
    /// </summary>
    public int OperationTypeLimit { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<DDailyUsage> DDailyUsages { get; set; } = new List<DDailyUsage>();
}
