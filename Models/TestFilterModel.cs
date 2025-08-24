using System;
using System.Collections.Generic;

namespace PhrazorApp.Models
{
    public sealed class TestFilterModel
    {
        // 複数選択（ジャンルは親表示のみのため保持不要。サブジャンルで絞り込む）
        public HashSet<Guid> SubGenreIds { get; set; } = new();
        public HashSet<Guid> PhraseBookIds { get; set; } = new();

        // 期間
        public DateRangePreset DatePreset { get; set; } = DateRangePreset.None;
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        // オプション
        public bool UntestedOnly { get; set; } = false;
        public bool Shuffle { get; set; } = true;

        // 出題数
        public int Limit { get; set; } = 20;
    }
}
