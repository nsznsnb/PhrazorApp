using MudBlazor;
using PhrazorApp.Constants;

namespace PhrazorApp.Themes
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
                Primary = Colors.Indigo.Default,
                Secondary = Colors.Cyan.Accent4,
                Info = Colors.Blue.Darken2,
                Success = Colors.Green.Accent4,
                Warning = Colors.Orange.Default,
                Error = Colors.Red.Accent2,
                AppbarBackground = Colors.Shades.White,
                Background = Colors.Gray.Lighten5,
                DrawerIcon = Colors.Cyan.Accent4
            },
            LayoutProperties = new LayoutProperties()
            {
            },
            Typography = new Typography()
            {
                Default = new DefaultTypography()
                {
                    FontFamily = new[] {
                    ComDefine.FONT_FAMILY_1,
                    ComDefine.FONT_FAMILY_2,
                    ComDefine.FONT_FAMILY_3
                }
                }
            }
        };
    }
}
