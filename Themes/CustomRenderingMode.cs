using Microsoft.AspNetCore.Components.Web;

namespace PhrazorApp.Themes
{
    /// <summary>
    /// Blazorのカスタムレンダリングモードを定義するクラス。
    /// </summary>
    public static class CustomRenderingMode
    {
        /// <summary>
        /// Serverモード
        /// </summary>
        public static InteractiveServerRenderMode InteractiveServer { get; } = new();
        /// <summary>
        /// Wasmモード
        /// </summary>
        public static InteractiveWebAssemblyRenderMode InteractiveWebAssembly { get; } = new();
        /// <summary>
        /// InteractiveAutoモード(初回ロード時サーバーモード+キャッシュ後Wasmモード)
        /// </summary>
        public static InteractiveAutoRenderMode InteractiveAuto { get; } = new();

        /// <summary>
        /// Serverモード(プリレンダリング無し)
        /// </summary>
        public static InteractiveServerRenderMode InteractiveServerWithourPrerendering { get; } = new(prerender: false);
        /// <summary>
        /// Wasmモード(プリレンダリング無し)
        /// </summary>
        public static InteractiveWebAssemblyRenderMode InteractiveWebAssemblyWithoutPrerendering { get; } = new(prerender: false);
        /// <summary>
        /// InteractiveAutoモード(プリレンダリング無し): 初回ロード時サーバーモード+キャッシュ後Wasmモード
        /// </summary>
        public static InteractiveAutoRenderMode InteractiveAutoWithourPrerendering { get; } = new(prerender: false);
    }
}
