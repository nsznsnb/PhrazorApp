using System;
using System.Collections.Generic;

namespace PhrazorApp.Models;

/// <summary>
/// 復習履歴
/// </summary>
public partial class DReviewLog
{
    /// <summary>
    /// 復習履歴ID
    /// </summary>
    public Guid ReviewId { get; set; }

    /// <summary>
    /// フレーズID
    /// </summary>
    public Guid PhraseId { get; set; }

    /// <summary>
    /// 復習日
    /// </summary>
    public DateTime? ReviewDate { get; set; }

    /// <summary>
    /// 復習種別ID
    /// </summary>
    public int? ReviewTypeId { get; set; }

    /// <summary>
    /// テスト結果ID
    /// </summary>
    public int TestId { get; set; }

    /// <summary>
    /// テスト結果明細連番
    /// </summary>
    public int TestResultDetailNo { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    public virtual DTestResultDetail DTestResultDetail { get; set; } = null!;

    public virtual DUserPhrase Phrase { get; set; } = null!;

    public virtual MReviewType? ReviewType { get; set; }
}
