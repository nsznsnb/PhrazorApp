using System;
using System.Collections.Generic;

namespace PhrazorApp.Data.Entities;

/// <summary>
/// ジャンルマスタ
/// </summary>
public partial class MGenre
{
    /// <summary>
    /// ジャンルID
    /// </summary>
    public Guid GenreId { get; set; }

    /// <summary>
    /// ジャンル名
    /// </summary>
    public string GenreName { get; set; } = null!;

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

    public virtual ICollection<MSubGenre> MSubGenres { get; set; } = new List<MSubGenre>();
}
