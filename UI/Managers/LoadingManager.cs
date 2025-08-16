namespace PhrazorApp.UI.Managers
{
    /// <summary>ローディング状態サービス（キャンセル機能なし版）</summary>

    public sealed class LoadingManager
    {
        public event Action? VisibilityChanged;

        public bool IsVisible { get; private set; }
        public string Message { get; private set; } = "処理中です…";
        public bool Indeterminate { get; private set; } = true;
        public double? Value { get; private set; }

        public OverlayScope Scope { get; private set; } = OverlayScope.BodyOnly;  

        public void Show(string? message = null, bool indeterminate = true, double? value = null,
                         OverlayScope scope = OverlayScope.BodyOnly)               
        {
            Message = string.IsNullOrWhiteSpace(message) ? "処理中です…" : message!;
            Indeterminate = indeterminate;
            Value = value;
            Scope = scope;                                                       // ← 追加
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
            Scope = OverlayScope.BodyOnly; // 既定に戻す
            VisibilityChanged?.Invoke();
        }
    }

}
