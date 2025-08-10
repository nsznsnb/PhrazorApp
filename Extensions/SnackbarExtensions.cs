using MudBlazor;

namespace PhrazorApp.Extensions
{
    /// <summary>
    /// トースト拡張メソッド
    /// </summary>
    public static class SnackbarExtensions
    {
        /// <summary>
        /// サービス結果をトースト表示する
        /// </summary>
        /// <param name="snackbar"></param>
        /// <param name="result">サービス結果</param>
        /// <returns></returns>
        public static Snackbar? AddServiceResult(this ISnackbar snackbar, IServiceResult result)
        {
            var message = result?.Message ?? string.Empty;

            // トースト右上表示
            snackbar.Configuration.PositionClass = Defaults.Classes.Position.TopRight;
            switch (result!.Status)
            {
                case ServiceStatus.Success:
                    snackbar.Add(message, Severity.Success);
                    break;
                case ServiceStatus.Warning:
                    snackbar.Add(message, Severity.Warning);
                    break;
                case ServiceStatus.Error:
                    // エラーの場合ユーザーがタッチするまで表示
                    snackbar.Configuration.RequireInteraction = true;
                    snackbar.Add(message, Severity.Error);
                    break;
            }
            return snackbar as Snackbar;
        }
    }
}
