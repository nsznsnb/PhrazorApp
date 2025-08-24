using System.Text.Json;
using PhrazorApp.UI.Interop;

namespace PhrazorApp.UI.State
{
    public sealed class ReviewSession
    {
        private JsInteropManager? _js;
        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        public IReadOnlyList<Card> Items { get; private set; } = Array.Empty<Card>();

        /// <summary>JS呼び出しを有効化（初回レンダー後にページから1回だけ呼ぶ）</summary>
        public void AttachJs(JsInteropManager js) => _js = js;

        // 既存：そのまま維持（呼び出し側は変更不要）
        public void Set<T>(IEnumerable<T> src, Func<T, string> front, Func<T, string> back)
        {
            Items = src.Select(x => new Card(Guid.Empty, front(x), back(x))).ToList();
            _ = SaveIfReadyAsync();
        }

        // ★ 追加：Id セレクタ付き（成績保存用に PhraseId を持たせたいときだけ使う）
        public void Set<T>(IEnumerable<T> src, Func<T, Guid> id, Func<T, string> front, Func<T, string> back)
        {
            Items = src.Select(x => new Card(id(x), front(x), back(x))).ToList();
            _ = SaveIfReadyAsync();
        }

        public void Clear()
        {
            Items = Array.Empty<Card>();
            _ = RemoveIfReadyAsync();
        }

        /// <summary>sessionStorage から復元（AttachJs 済みのときにのみ動く）</summary>
        public async Task RestoreAsync(CancellationToken ct = default)
        {
            if (_js is null) return;
            var json = await _js.SessionGetAsync(SessionStorageKey.ReviewStateV1, ct);
            if (string.IsNullOrWhiteSpace(json)) return;

            var data = JsonSerializer.Deserialize<Persist>(json, _json);
            Items = data?.Items?.ToArray() ?? Array.Empty<Card>();
        }

        // ★ Card を拡張（Front/Back アクセスはそのまま動く）
        public readonly record struct Card(Guid PhraseId, string Front, string Back);

        private async Task SaveIfReadyAsync(CancellationToken ct = default)
        {
            if (_js is null) return;
            var payload = new Persist { Items = Items.ToList(), SavedAtUtc = DateTime.UtcNow };
            var json = JsonSerializer.Serialize(payload, _json);
            await _js.SessionSetAsync(SessionStorageKey.ReviewStateV1, json, ct);
        }

        private async Task RemoveIfReadyAsync(CancellationToken ct = default)
        {
            if (_js is null) return;
            await _js.SessionRemoveAsync(SessionStorageKey.ReviewStateV1, ct);
        }

        private sealed class Persist
        {
            public List<Card> Items { get; set; } = new();
            public DateTime SavedAtUtc { get; set; }
        }
    }
}
