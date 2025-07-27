using System;
using System.Collections.Generic;

namespace PhrazorApp.Data.Entities;

/// <summary>
/// テスト結果明細
/// </summary>
public partial class DTestResultDetail
{
    /// <summary>
    /// テスト結果ID
    /// </summary>
    public int TestId { get; set; }

    /// <summary>
    /// テスト結果明細連番
    /// </summary>
    public int TestResultDetailNo { get; set; }

    /// <summary>
    /// フレーズID
    /// </summary>
    public Guid PhraseId { get; set; }

    /// <summary>
    /// 正解フラグ
    /// </summary>
    public bool? IsCorrect { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<DReviewLog> DReviewLogs { get; set; } = new List<DReviewLog>();

    public virtual DPhrase Phrase { get; set; } = null!;

    public virtual DTestResult Test { get; set; } = null!;
}
