namespace PhrazorApp.Components.State
{
    public sealed class ReviewSession
    {
        public IReadOnlyList<Card> Items { get; private set; } = Array.Empty<Card>();

        // 既存：そのまま維持（呼び出し側は変更不要）
        public void Set<T>(IEnumerable<T> src, Func<T, string> front, Func<T, string> back)
            => Items = src.Select(x => new Card(Guid.Empty, front(x), back(x))).ToList();

        // ★ 追加：Id セレクタ付き（成績保存用に PhraseId を持たせたいときだけ使う）
        public void Set<T>(IEnumerable<T> src, Func<T, Guid> id, Func<T, string> front, Func<T, string> back)
            => Items = src.Select(x => new Card(id(x), front(x), back(x))).ToList();

        public void Clear() => Items = Array.Empty<Card>();

        // ★ Card を拡張（Front/Back アクセスはそのまま動く）
        public readonly record struct Card(Guid PhraseId, string Front, string Back);
    }
}
