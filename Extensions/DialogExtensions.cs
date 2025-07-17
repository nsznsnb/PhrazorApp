using MudBlazor;
using PhrazorApp.Components.Shared;

namespace PhrazorApp.Extensions
{
    /// <summary>
    /// ダイアログ拡張クラス
    /// </summary>
    public static class DialogExtensions
    {
        /// <summary>
        /// 共通ダイアログを表示する
        /// </summary>
        /// <param name="dialogService"></param>
        /// <param name="dialogPattern">ダイアログ種別</param>
        /// <param name="content">ダイアログ本文</param>
        /// <returns></returns>
        public static Task<IDialogReference> ShowCommonDialogAsync(this IDialogService dialogService, CommonDialogPattern dialogPattern, string content)
        {
            var parameters = new DialogParameters<CommonDialog>
            {
                { x => x.DialogPattern , dialogPattern },
                { x => x.ContentText, content },
            };

            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall, Position = DialogPosition.TopCenter };

            return dialogService.ShowAsync<CommonDialog>(string.Empty, parameters, options);
        }
    }
}
