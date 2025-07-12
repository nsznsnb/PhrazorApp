using System;
using System.Collections.Generic;

namespace PhrazorApp.Models;

/// <summary>
/// フレーズ帳内フレーズ
/// </summary>
public partial class DPhraseBookItem
{
    /// <summary>
    /// フレーズ帳Id
    /// </summary>
    public Guid PhraseBookId { get; set; }

    /// <summary>
    /// フレーズId
    /// </summary>
    public Guid PhraseId { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    public virtual DUserPhrase Phrase { get; set; } = null!;

    public virtual MPhraseBook PhraseBook { get; set; } = null!;
}
