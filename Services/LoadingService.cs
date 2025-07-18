namespace PhrazorApp.Services
{
    /// <summary>
    /// ローディング状態サービス
    /// </summary>
    public class LoadingService
    {
        public event Action OnChange;

        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            private set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnChange?.Invoke();
                }
            }
        }

        public void Show() => IsVisible = true;
        public void Hide() => IsVisible = false;
    }
}
