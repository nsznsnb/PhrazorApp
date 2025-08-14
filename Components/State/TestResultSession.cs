namespace PhrazorApp.Components.State
{
    public sealed class TestResultSession
    {
        public sealed record Row(string Front, string Back, bool IsCorrect);

        public int Total { get; private set; }
        public int Correct { get; private set; }
        public int Wrong => Total - Correct;
        public List<Row> Items { get; private set; } = new();

        public void Set(IEnumerable<Row> rows)
        {
            Items = rows.ToList();
            Total = Items.Count;
            Correct = Items.Count(x => x.IsCorrect);
        }

        public void Clear()
        {
            Items.Clear(); Total = Correct = 0;
        }

        public double Rate() => Total == 0 ? 0 : (double)Correct / Total;
        public List<Row> WrongOnly() => Items.Where(x => !x.IsCorrect).ToList();
    }
}
