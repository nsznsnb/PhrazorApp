namespace PhrazorApp.Services
{
    /// <summary>
    /// ローディング状態サービス
    /// </summary>
    public sealed class LoadingService
    {
        public event Action? VisibilityChanged;

        public bool IsVisible { get; private set; }
        public string Message { get; private set; } = "処理中です…";
        public bool ShowCancel { get; private set; }
        public bool Indeterminate { get; private set; } = true;
        public double? Value { get; private set; }

        // キャンセル要求時に呼ぶコールバック（CT を外から結びつける想定）
        private Action? _onCancel;

        public void Show(
            string? message = null,
            bool showCancel = false,
            Action? onCancel = null,
            bool indeterminate = true,
            double? value = null)
        {
            Message = string.IsNullOrWhiteSpace(message) ? "処理中です…" : message!;
            ShowCancel = showCancel;
            _onCancel = onCancel;
            Indeterminate = indeterminate;
            Value = value;

            IsVisible = true;
            VisibilityChanged?.Invoke();
        }

        public void Update(string? message = null, double? value = null, bool? indeterminate = null)
        {
            if (!string.IsNullOrWhiteSpace(message)) Message = message!;
            if (value.HasValue) Value = value;
            if (indeterminate.HasValue) Indeterminate = indeterminate.Value;
            VisibilityChanged?.Invoke();
        }

        public void Hide()
        {
            IsVisible = false;
            _onCancel = null;
            VisibilityChanged?.Invoke();
        }

        public void RequestCancel() => _onCancel?.Invoke();
    }
}
