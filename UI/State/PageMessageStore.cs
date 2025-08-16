using MudBlazor;
using System.Collections.Immutable;

namespace PhrazorApp.UI.State
{
    /// ページ単位メッセージのストア（UIなし）
    public sealed class PageMessageStore
    {
        private readonly List<PageMessage> _messages = new();
        public event Action? Changed;

        public IReadOnlyList<PageMessage> Items => _messages.ToImmutableArray();
        public bool HasAny => _messages.Count > 0;

        public void Add(Severity severity, string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            _messages.Add(new PageMessage(severity, text));
            Changed?.Invoke();
        }

        public void AddRange(Severity severity, IEnumerable<string>? messages)
        {
            if (messages is null) return;
            var added = false;
            foreach (var m in messages)
            {
                if (string.IsNullOrWhiteSpace(m)) continue;
                _messages.Add(new PageMessage(severity, m));
                added = true;
            }
            if (added) Changed?.Invoke();
        }

        public void Error(string text) => Add(Severity.Error, text);
        public void Warning(string text) => Add(Severity.Warning, text);
        public void Info(string text) => Add(Severity.Info, text);
        public void Success(string text) => Add(Severity.Success, text);

        public void Errors(IEnumerable<string>? messages) => AddRange(Severity.Error, messages);
        public void Warnings(IEnumerable<string>? messages) => AddRange(Severity.Warning, messages);
        public void Infos(IEnumerable<string>? messages) => AddRange(Severity.Info, messages);
        public void Successes(IEnumerable<string>? messages) => AddRange(Severity.Success, messages);

        public void Remove(PageMessage message)
        {
            if (_messages.Remove(message)) Changed?.Invoke();
        }

        public void Clear()
        {
            if (_messages.Count == 0) return;
            _messages.Clear();
            Changed?.Invoke();
        }
    }

    public sealed record PageMessage(Severity Severity, string Text);
}
