using System;
using System.Collections.Generic;

namespace PhrazorApp.Models;

/// <summary>
/// フレーズ帳マスタ
/// </summary>
public partial class MPhraseBook
{
    /// <summary>
    /// フレーズ帳Id
    /// </summary>
    public Guid PhraseBookId { get; set; }

    /// <summary>
    /// フレーズ帳名
    /// </summary>
    public string PhraseBookName { get; set; } = null!;

    /// <summary>
    /// ユーザーId
    /// </summary>
    public string UserId { get; set; } = null!;

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<DPhraseBookItem> DPhraseBookItems { get; set; } = new List<DPhraseBookItem>();
}
