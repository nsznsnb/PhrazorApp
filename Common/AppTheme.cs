using MudBlazor;

namespace PhrazorApp.Common
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
                Info = Colors.Blue.Default,
                AppbarBackground = Colors.Indigo.Default,
                Background = Colors.Gray.Lighten5,
            },
            LayoutProperties = new LayoutProperties()
            {
                AppbarHeight = "54px"
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
