using System;
using System.Collections.Generic;

namespace PhrazorApp.Data.Entities;

/// <summary>
/// フレーズ画像
/// </summary>
public partial class DPhraseImage
{
    /// <summary>
    /// フレーズ画像ID
    /// </summary>
    public Guid PhraseImageId { get; set; }

    /// <summary>
    /// URL
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// アップロード日時
    /// </summary>
    public DateTime? UploadAt { get; set; }

    /// <summary>
    /// フレーズID
    /// </summary>
    public Guid PhraseId { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual DPhrase Phrase { get; set; } = null!;
}
