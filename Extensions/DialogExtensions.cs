using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhrazorApp.Components.Shared.Dialogs;
using System.Linq.Expressions;

public static class DialogServiceExtensions
{
    // よく使う既定オプション
    public static DialogOptions OptionsXs() => new()
    {
        CloseButton = true,
        MaxWidth = MaxWidth.ExtraSmall,
        Position = DialogPosition.TopCenter,
        FullWidth = true
    };
    public static DialogOptions OptionsSm() => new()
    {
        CloseButton = true,
        MaxWidth = MaxWidth.Small,
        Position = DialogPosition.TopCenter,
        FullWidth = true
    };
    public static DialogOptions OptionsMd() => new()
    {
        CloseButton = true,
        MaxWidth = MaxWidth.Medium,
        Position = DialogPosition.TopCenter,
        FullWidth = true
    };

    /// <summary>
    /// 値型パラメータを1つ渡す（強い型安全）
    /// </summary>
    public static Task<IDialogReference> ShowWithAsync<TDialog, TProp>(
            this IDialogService dialogService,
            string title,
            Expression<Func<TDialog, TProp>> paramSelector, // ★ object? → TProp
            TProp value,
            DialogOptions? options = null)
            where TDialog : IComponent
    {
        var parameters = new DialogParameters<TDialog>
        {
            { paramSelector, value }
        };
        return dialogService.ShowAsync<TDialog>(title, parameters, options ?? OptionsSm());
    }

    /// <summary>
    /// EventCallback を1つ渡す（強い型安全）
    /// </summary>
    public static Task<IDialogReference> ShowWithCallbackAsync<TDialog, TArg>(
        this IDialogService dialogService,
        string title,
        object caller,
        Expression<Func<TDialog, EventCallback<TArg>>> callbackSelector, // ★ object? → EventCallback<TArg>
        Func<TArg, Task> handler,
        DialogOptions? options = null)
        where TDialog : IComponent
    {
        var parameters = new DialogParameters<TDialog>
        {
            { callbackSelector, EventCallback.Factory.Create(caller, handler) }
        };
        return dialogService.ShowAsync<TDialog>(title, parameters, options ?? OptionsSm());
    }


    /// <summary>
    /// 値×N と EventCallback×1 をまとめて渡す（CSV等の実用向け）
    /// </summary>
    public static Task<IDialogReference> ShowWithParamsAndCallbackAsync<TDialog, TArg>(
        this IDialogService dialogService,
        string title,
        object caller,
        (Expression<Func<TDialog, object?>> Selector, object? Value)[] values,
        Expression<Func<TDialog, EventCallback<TArg>>> callbackSelector, // ★ ここを強く型付け
        Func<TArg, Task> handler,
        DialogOptions? options = null)
        where TDialog : IComponent
    {
        var parameters = new DialogParameters<TDialog>();
        foreach (var (sel, val) in values)
            parameters.Add(sel, val);

        // ★ EventCallback<TArg> を正しく生成して渡す
        parameters.Add(callbackSelector, EventCallback.Factory.Create(caller, handler));

        return dialogService.ShowAsync<TDialog>(title, parameters, options ?? OptionsSm());
    }

    /// <summary>
    /// 既存の「共通確認ダイアログ」
    /// </summary>
    private static Task<IDialogReference> ShowDialogCommonAsync(
        this IDialogService dialogService,
        DialogConfirmType dialogPattern,
        string content)
    {
        var parameters = new DialogParameters<DialogCommon>
        {
            { x => x.DialogPattern , dialogPattern },
            { x => x.ContentText, content },
        };
        return dialogService.ShowAsync<DialogCommon>(string.Empty, parameters, OptionsXs());
    }

    public static async Task<bool> ShowConfirmAsync(
        this IDialogService dialogService,
        DialogConfirmType type,
        string message)
    {
        var dialog = await dialogService.ShowDialogCommonAsync(type, message);
        var result = await dialog.Result;
        // OK → true / Cancel or Close → false
        return !(result?.Canceled ?? true);
    }

    /// <summary>
    /// CSVアップロードダイアログ（糖衣メソッド）
    /// </summary>
    public static Task<IDialogReference> ShowCsvUploadDialogAsync<TDialog, TItem>(
        this IDialogService dialogService,
        object caller,
        Func<List<TItem>, Task> onUploadCompleted,
        string title = "CSV読込",
        DialogOptions? options = null)
        where TDialog : DialogCsvUploadBase<TItem>, IComponent
    {
        return dialogService.ShowWithCallbackAsync<TDialog, List<TItem>>(
            title,
            caller,
            x => x.OnUploadCompleted, // ★ ここが MemberExpression で解釈される
            onUploadCompleted,
            options ?? OptionsSm());
    }
}
