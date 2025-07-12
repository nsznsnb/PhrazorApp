using Microsoft.AspNetCore.Components.Web;

namespace PhrazorApp.Common
{
    /// <summary>
    /// Blazorのカスタムレンダリングモードを定義するクラス。
    /// </summary>
    public static class CustomRenderingMode
    {
        public static InteractiveServerRenderMode InteractiveServer { get; } = new();
        public static InteractiveWebAssemblyRenderMode InteractiveWebAssembly { get; } = new();
        public static InteractiveAutoRenderMode InteractiveAuto { get; } = new();

        public static InteractiveServerRenderMode InteractiveServerWithourPrerendering { get; } = new(prerender: false);
        public static InteractiveWebAssemblyRenderMode InteractiveWebAssemblyWithoutPrerendering { get; } = new(prerender: false);
        public static InteractiveAutoRenderMode InteractiveAutoWithourPrerendering { get; } = new(prerender: false);
    }
}
