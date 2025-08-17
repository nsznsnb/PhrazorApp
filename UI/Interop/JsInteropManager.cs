using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace PhrazorApp.UI.Interop;

public sealed class JsInteropManager : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly ILogger<JsInteropManager> _logger;
    private IJSObjectReference? _module;

    public JsInteropManager(IJSRuntime js, ILogger<JsInteropManager> logger)
    {
        _js = js;
        _logger = logger;
    }

    // site.js を ES Module として読み込み
    private async ValueTask<IJSObjectReference> GetModuleAsync(CancellationToken ct = default)
    {
        if (_module is null)
            _module = await _js.InvokeAsync<IJSObjectReference>("import", ct, "./js/site.js");
        return _module;
    }

    /// <summary>Enter で指定 ID へフォーカス移動</summary>
    public async ValueTask HandleKeyDownToFocusAsync(KeyboardEventArgs e, string targetElementId, CancellationToken ct = default)
    {
        if (e.Key != "Enter" || string.IsNullOrWhiteSpace(targetElementId)) return;
        await FocusByIdAsync(targetElementId, ct);
    }

    /// <summary>ID 指定でフォーカス移動</summary>
    public async ValueTask FocusByIdAsync(string targetElementId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(targetElementId)) return;
        try
        {
            var mod = await GetModuleAsync(ct);
            await mod.InvokeVoidAsync("focusElementById", ct, targetElementId);
        }
        catch (JSException ex)
        {
            _logger.LogWarning(ex, "JS focusElementById 失敗: {Id}", targetElementId);
        }
    }

    /// <summary>ID 指定でスムーズスクロール（Home遷移しない）</summary>
    public async ValueTask ScrollToIdAsync(string targetElementId, bool smooth = true, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(targetElementId)) return;
        try
        {
            var mod = await GetModuleAsync(ct);
            await mod.InvokeVoidAsync("scrollToId", ct, targetElementId, smooth);
        }
        catch (JSException ex)
        {
            _logger.LogWarning(ex, "JS scrollToId 失敗: {Id}", targetElementId);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try { await _module.DisposeAsync(); } catch { /* no-op */ }
        }
    }
}
