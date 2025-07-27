using System;
using System.Collections.Generic;

namespace PhrazorApp.Data.Entities;

/// <summary>
/// テスト結果
/// </summary>
public partial class DTestResult
{
    /// <summary>
    /// テスト結果ID
    /// </summary>
    public int TestId { get; set; }

    /// <summary>
    /// テスト日時
    /// </summary>
    public DateTime TestDatetime { get; set; }

    /// <summary>
    /// 成績ID
    /// </summary>
    public short? GradeId { get; set; }

    /// <summary>
    /// 完了フラグ
    /// </summary>
    public bool CompleteFlg { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<DTestResultDetail> DTestResultDetails { get; set; } = new List<DTestResultDetail>();

    public virtual MGrade? Grade { get; set; }
}
