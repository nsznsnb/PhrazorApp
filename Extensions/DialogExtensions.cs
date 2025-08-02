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
        public static Task<IDialogReference> ShowDialogCommonAsync(this IDialogService dialogService, DialogConfirmType dialogPattern, string content)
        {
            var parameters = new DialogParameters<DialogCommon>
            {
                { x => x.DialogPattern , dialogPattern },
                { x => x.ContentText, content },
            };

            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall, Position = DialogPosition.TopCenter, FullWidth = true };

            return dialogService.ShowAsync<DialogCommon>(string.Empty, parameters, options);
        }
    }
}
