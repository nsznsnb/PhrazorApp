using System;
using System.Collections.Generic;

namespace PhrazorApp.Data.Entities;

/// <summary>
/// フレーズジャンル
/// </summary>
public partial class MPhraseGenre
{
    /// <summary>
    /// フレーズID
    /// </summary>
    public Guid PhraseId { get; set; }

    /// <summary>
    /// サブジャンルID
    /// </summary>
    public Guid SubGenreId { get; set; }

    /// <summary>
    /// ジャンルID
    /// </summary>
    public Guid GenreId { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    public virtual MSubGenre MSubGenre { get; set; } = null!;

    public virtual DPhrase Phrase { get; set; } = null!;
}
