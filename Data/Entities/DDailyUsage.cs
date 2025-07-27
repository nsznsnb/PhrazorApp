using System;
using System.Collections.Generic;

namespace PhrazorApp.Data.Entities;

/// <summary>
/// 操作履歴
/// </summary>
public partial class DDailyUsage
{
    /// <summary>
    /// ユーザーID
    /// </summary>
    public string UserId { get; set; } = null!;

    /// <summary>
    /// 操作日
    /// </summary>
    public DateOnly OperationDate { get; set; }

    /// <summary>
    /// 操作種別ID
    /// </summary>
    public Guid OperationTypeId { get; set; }

    /// <summary>
    /// 操作回数
    /// </summary>
    public int? OperationCount { get; set; }

    public virtual MOperationType OperationType { get; set; } = null!;
}
