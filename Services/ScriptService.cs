using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using static MudBlazor.CategoryTypes;

namespace PhrazorApp.Services
{
    public class ScriptService
    {
        private readonly IJSRuntime _js;

        public ScriptService(IJSRuntime js)
        {
            _js = js;
        }

        /// <summary>
        /// Enterキー押下時に指定のIdにフォーカス移動
        /// </summary>
        /// <param name="e"></param>
        /// <param name="targetElementId"></param>
        /// <returns></returns>
        public async ValueTask HandleKeyDownToFocusAsync(KeyboardEventArgs e, string targetElementId)
        {
            if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(targetElementId))
            {
                try
                {
                    await _js.InvokeVoidAsync("focusHelper.focusElementById", targetElementId);
                }
                catch (JSException ex)
                {
                    Console.WriteLine($"JavaScript error: {ex.Message}");
                }
            }
        }
    }
}
