using System;
using System.Collections.Generic;

namespace PhrazorApp.Data.Entities;

/// <summary>
/// サブジャンルマスタ
/// </summary>
public partial class MSubGenre
{
    /// <summary>
    /// ジャンルID
    /// </summary>
    public Guid GenreId { get; set; }

    /// <summary>
    /// サブジャンルID
    /// </summary>
    public Guid SubGenreId { get; set; }

    /// <summary>
    /// サブジャンル名
    /// </summary>
    public string SubGenreName { get; set; } = null!;

    /// <summary>
    /// ソート順
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// ユーザーId
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

    public virtual MGenre Genre { get; set; } = null!;

    public virtual ICollection<MPhraseGenre> MPhraseGenres { get; set; } = new List<MPhraseGenre>();
}
