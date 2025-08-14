// Models/TestFilterModel.cs

namespace PhrazorApp.Models
{
    public sealed class TestFilterModel
    {
        public List<Guid> PhraseBookIds { get; set; } = new(); // ★複数選択
        public Guid? GenreId { get; set; }
        public Guid? SubGenreId { get; set; }

        public DateRangePreset DatePreset { get; set; } = DateRangePreset.None;
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public bool UntestedOnly { get; set; } = false;
        public int Limit { get; set; } = 20;
        public bool Shuffle { get; set; } = true;
    }
}
