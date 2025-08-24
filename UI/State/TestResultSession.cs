using System.Text.Json;
using PhrazorApp.UI.Interop;

namespace PhrazorApp.UI.State
{
    public sealed class TestResultSession
    {
        private JsInteropManager? _js;
        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        public sealed record Row(string Front, string Back, bool IsCorrect, Guid PhraseId);

        public int Total { get; private set; }
        public int Correct { get; private set; }
        public int Wrong => Total - Correct;
        public List<Row> Items { get; private set; } = new();

        /// <summary>JS呼び出しを有効化（初回レンダー後にページから1回だけ呼ぶ）</summary>
        public void AttachJs(JsInteropManager js) => _js = js;

        public void Set(IEnumerable<Row> rows)
        {
            Items = rows.ToList();
            Total = Items.Count;
            Correct = Items.Count(x => x.IsCorrect);
            _ = SaveIfReadyAsync();
        }

        public void Clear()
        {
            Items.Clear();
            Total = Correct = 0;
            _ = RemoveIfReadyAsync();
        }

        public double Rate() => Total == 0 ? 0 : (double)Correct / Total;
        public List<Row> WrongOnly() => Items.Where(x => !x.IsCorrect).ToList();

        /// <summary>sessionStorage から復元（AttachJs 済みのときにのみ動く）</summary>
        public async Task RestoreAsync(CancellationToken ct = default)
        {
            if (_js is null) return;
            var json = await _js.SessionGetAsync(SessionStorageKey.ReviewResultV1, ct);
            if (string.IsNullOrWhiteSpace(json)) return;

            var data = JsonSerializer.Deserialize<Persist>(json, _json);
            if (data is null) return;

            Items = data.Items ?? new();
            Total = data.Total;
            Correct = data.Correct;
        }

        private async Task SaveIfReadyAsync(CancellationToken ct = default)
        {
            if (_js is null) return;
            var payload = new Persist
            {
                Items = Items,
                Total = Total,
                Correct = Correct,
                SavedAtUtc = DateTime.UtcNow
            };
            var json = JsonSerializer.Serialize(payload, _json);
            await _js.SessionSetAsync(SessionStorageKey.ReviewResultV1, json, ct);
        }

        private async Task RemoveIfReadyAsync(CancellationToken ct = default)
        {
            if (_js is null) return;
            await _js.SessionRemoveAsync(SessionStorageKey.ReviewResultV1, ct);
        }

        private sealed class Persist
        {
            public List<Row> Items { get; set; } = new();
            public int Total { get; set; }
            public int Correct { get; set; }
            public DateTime SavedAtUtc { get; set; }
        }
    }
}
