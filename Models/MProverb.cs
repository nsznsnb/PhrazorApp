using System;
using System.Collections.Generic;

namespace PhrazorApp.Models;

/// <summary>
/// 格言マスタ
/// </summary>
public partial class MProverb
{
    /// <summary>
    /// 格言ID
    /// </summary>
    public Guid ProverbId { get; set; }

    /// <summary>
    /// 格言
    /// </summary>
    public string ProverbText { get; set; } = null!;

    /// <summary>
    /// 意味
    /// </summary>
    public string? Meaning { get; set; }

    /// <summary>
    /// 著者
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
