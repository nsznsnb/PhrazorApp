namespace PhrazorApp.Commons.Constants
{
    public static class AppZIndex
    {
        public const int Drawer = 1100;
        public const int Popover = 1200;
        public const int ContentOverlay = 1250;  // ★ MudMainContentだけ覆う用
        public const int AppBar = 1300;
        public const int Dialog = 1400;
        public const int GlobalOverlay = 1450;   // ★ 全画面（ダイアログより前, Snackbarより後）
        public const int Snackbar = 1500;
        public const int Tooltip = 1600;
    }
}
