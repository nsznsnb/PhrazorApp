using MudBlazor;

namespace PhrazorApp.UI.Themes
{
    /// <summary>
    /// アプリのテーマ設定
    /// </summary>
    public static class AppTheme
    {
        public static readonly MudTheme MyCustomTheme = new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = Colors.Teal.Default,
                Secondary = Colors.Cyan.Accent4,
                Info = Colors.Blue.Lighten1,
                Success = Colors.Green.Accent4,
                Warning = Colors.Orange.Default,
                Error = Colors.Red.Accent2,
                // 背景系
                AppbarBackground = Colors.Shades.White,
                Background = Colors.Gray.Lighten5,
                Surface = Colors.Shades.White,            // カードやDialogの背景

                // テキスト系
                TextPrimary = Colors.Gray.Darken4,
                TextSecondary = Colors.Gray.Darken2,

                // アクション（IconやDisabled状態など）
                ActionDefault = Colors.Gray.Darken1,
                ActionDisabled = Colors.Gray.Lighten1,
                ActionDisabledBackground = Colors.Gray.Lighten3,

                // Drawerやナビゲーション
                DrawerBackground = Colors.Gray.Lighten4,
                DrawerText = Colors.Gray.Darken3,
                DrawerIcon = Colors.Cyan.Accent4,
            },
            ZIndex = new ZIndex() {
                Drawer = AppZIndex.Drawer,
                Popover = AppZIndex.Popover,
                AppBar = AppZIndex.AppBar,
                Dialog = AppZIndex.Dialog,
                Snackbar = AppZIndex.Snackbar,
                Tooltip = AppZIndex.Tooltip
            },
            LayoutProperties = new LayoutProperties()
            {
            },
            Typography = new Typography()
            {
                Default = new DefaultTypography()
                {
                    FontFamily = new[] {
                    AppConstants.FONT_FAMILY_1,
                    AppConstants.FONT_FAMILY_2,
                    AppConstants.FONT_FAMILY_3
                }
                }
            }
        };
    }
}
