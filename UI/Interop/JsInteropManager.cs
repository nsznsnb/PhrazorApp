using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace PhrazorApp.UI.Interop;

public sealed class JsInteropManager : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly ILogger<JsInteropManager> _logger;

    // ESM の遅延ロード用（並行呼び出しでも一度だけ import）
    private Task<IJSObjectReference>? _moduleTask;

    public JsInteropManager(IJSRuntime js, ILogger<JsInteropManager> logger)
    {
        _js = js;
        _logger = logger;
    }

    /// <summary>site.js（ESM）を必要時に import して返す</summary>
    private Task<IJSObjectReference> EnsureModuleAsync(CancellationToken ct = default)
        => _moduleTask ??= _js.InvokeAsync<IJSObjectReference>("import", ct, "./js/site.js").AsTask();

    // ▼ 以下、すべてのメソッドはこのパターンで記述する
    // var mod = await EnsureModuleAsync(ct); 
    // await mod.InvokeVoidAsync("関数名", ct, args...);

    /// <summary>明示的に先読みしたい場合に呼ぶだけのヘルパ</summary>
    public async ValueTask EnsureLoadedAsync(CancellationToken ct = default)
        => await EnsureModuleAsync(ct);

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
            var mod = await EnsureModuleAsync(ct);
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
            var mod = await EnsureModuleAsync(ct);
            await mod.InvokeVoidAsync("scrollToId", ct, targetElementId, smooth);
        }
        catch (JSException ex)
        {
            _logger.LogWarning(ex, "JS scrollToId 失敗: {Id}", targetElementId);
        }
    }

    public async ValueTask HistoryBackAsync(string? fallbackUrl = null, CancellationToken ct = default)
    {
        try
        {
            var mod = await EnsureModuleAsync(ct);
            await mod.InvokeVoidAsync("historyBack", ct, fallbackUrl);
        }
        catch (JSException ex)
        {
            _logger.LogWarning(ex, "JS historyBack 失敗");
        }
    }

    /// <summary>要素の可視状態を監視（IntersectionObserver）。戻り値は監視ID。</summary>
    public async ValueTask<string?> ObserveElementVisibilityAsync(
        string elementId,
        DotNetObjectReference<object> dotNetRef,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(elementId)) return null;
        try
        {
            var mod = await EnsureModuleAsync(ct);
            return await mod.InvokeAsync<string?>("observeElementVisibility", ct, elementId, dotNetRef);
        }
        catch (JSException ex)
        {
            _logger.LogWarning(ex, "JS observeElementVisibility 失敗: {Id}", elementId);
            return null;
        }
    }

    public async ValueTask UnobserveElementVisibilityAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id)) return;
        try
        {
            var mod = await EnsureModuleAsync(ct);
            await mod.InvokeVoidAsync("unobserveElementVisibility", ct, id);
        }
        catch (JSException ex)
        {
            _logger.LogWarning(ex, "JS unobserveElementVisibility 失敗: {Id}", id);
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_moduleTask?.IsCompletedSuccessfully == true)
            {
                var module = await _moduleTask;
                await module.DisposeAsync();
            }
        }
        catch
        {
            // no-op
        }
    }
}
