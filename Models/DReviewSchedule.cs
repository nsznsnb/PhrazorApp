using System;
using System.Collections.Generic;

namespace PhrazorApp.Models;

/// <summary>
/// 復習スケジュール
/// </summary>
public partial class DReviewSchedule
{
    /// <summary>
    /// スケジュールID
    /// </summary>
    public Guid ScheduleId { get; set; }

    /// <summary>
    /// フレーズID
    /// </summary>
    public Guid PhraseId { get; set; }

    /// <summary>
    /// 次回復習日
    /// </summary>
    public DateTime NextReviewDate { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    public virtual DUserPhrase Phrase { get; set; } = null!;
}
