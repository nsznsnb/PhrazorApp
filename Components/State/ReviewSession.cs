namespace PhrazorApp.Components.State
{
    public sealed class ReviewSession
    {
        public IReadOnlyList<Card> Items { get; private set; } = Array.Empty<Card>();
        public void Set<T>(IEnumerable<T> src, Func<T, string> front, Func<T, string> back)
            => Items = src.Select(x => new Card(front(x), back(x))).ToList();
        public void Clear() => Items = Array.Empty<Card>();
        public readonly record struct Card(string Front, string Back);
    }
}
