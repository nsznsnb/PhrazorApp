using System;
using System.Collections.Generic;

namespace PhrazorApp.Data.Entities;

/// <summary>
/// フレーズ帳アイテム
/// </summary>
public partial class MPhraseBookItem
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
    /// ソート順
    /// </summary>
    public int? OrderNo { get; set; }

    /// <summary>
    /// メモ
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    public virtual DPhrase Phrase { get; set; } = null!;

    public virtual MPhraseBook PhraseBook { get; set; } = null!;
}
