using System;
using System.Collections.Generic;

namespace PhrazorApp.Models.Entities;

/// <summary>
/// ユーザーフレーズ
/// </summary>
public partial class DUserPhrase
{
    /// <summary>
    /// フレーズID
    /// </summary>
    public Guid PhraseId { get; set; }

    /// <summary>
    /// フレーズ
    /// </summary>
    public string? Phrase { get; set; }

    /// <summary>
    /// 意味
    /// </summary>
    public string? Meaning { get; set; }

    /// <summary>
    /// 備考
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// ユーザーID
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

    public virtual ICollection<DPhraseBookItem> DPhraseBookItems { get; set; } = new List<DPhraseBookItem>();

    public virtual ICollection<DPhraseImage> DPhraseImages { get; set; } = new List<DPhraseImage>();

    public virtual ICollection<DReviewLog> DReviewLogs { get; set; } = new List<DReviewLog>();

    public virtual ICollection<DTestResultDetail> DTestResultDetails { get; set; } = new List<DTestResultDetail>();

    public virtual ICollection<MPhraseCategory> MPhraseCategories { get; set; } = new List<MPhraseCategory>();
}
