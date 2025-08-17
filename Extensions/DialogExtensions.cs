using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhrazorApp.Components.Shared.Dialogs;
using System.Linq.Expressions;
using static PhrazorApp.Utils.CsvUtil;

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
        string content,
        string title = "",
        string executeButtonText = "")
    {
        var parameters = new DialogParameters<CommonDialog>
        {
            { x => x.DialogPattern , dialogPattern },
            { x => x.ContentText, content },
            { x => x.TitleText, title },
            { x => x.ExecuteButtonText, executeButtonText },
        };
        return dialogService.ShowAsync<CommonDialog>(string.Empty, parameters, OptionsXs());
    }

    public static async Task<bool> ShowConfirmAsync(
        this IDialogService dialogService,
        DialogConfirmType type,
        string message,
        string title = "",
        string executeButtonText = "")
    {
        var dialog = await dialogService.ShowDialogCommonAsync(type, message, title, executeButtonText);
        var result = await dialog.Result;
        // OK → true / Cancel or Close → false
        return !(result?.Canceled ?? true);
    }
    /// <summary>
    /// CSVアップロードダイアログ（必要最小限API）
    /// - タイトル/ボタン文言は固定（ダイアログ既定値を使用）
    /// - Schema は必須で明示指定
    /// - processAsync は任意注入
    /// </summary>
    public static Task<IDialogReference> ShowCsvUploadDialogAsync<TItem>(
        this IDialogService dialogService,
        object caller,
        Func<List<TItem>, Task> onUploadCompleted,
        CsvSchema schema,
        Func<List<TItem>, Task<ServiceResult<Unit>>>? processAsync = null,
        DialogOptions? options = null)
    {
        var parameters = new DialogParameters<CsvUploadDialog<TItem>>
        {
            { x => x.Schema, schema }, // ★ 必須
            { x => x.OnUploadCompleted, EventCallback.Factory.Create(caller, onUploadCompleted) }
        };
        if (processAsync is not null)
            parameters.Add(x => x.ProcessAsync, processAsync);

        // タイトルは固定表示（"CSV取込"）。ボタン文言もダイアログ既定値「取込」を使用。
        return dialogService.ShowAsync<CsvUploadDialog<TItem>>("CSV取込", parameters, options ?? OptionsSm());
    }
}
