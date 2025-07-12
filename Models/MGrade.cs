using System;
using System.Collections.Generic;

namespace PhrazorApp.Models;

/// <summary>
/// 成績マスタ
/// </summary>
public partial class MGrade
{
    /// <summary>
    /// 成績ID
    /// </summary>
    public short GradeId { get; set; }

    /// <summary>
    /// 成績名
    /// </summary>
    public string GradeName { get; set; } = null!;

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<DTestResult> DTestResults { get; set; } = new List<DTestResult>();
}
