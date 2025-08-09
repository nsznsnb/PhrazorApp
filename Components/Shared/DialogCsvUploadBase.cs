using Microsoft.AspNetCore.Components;

namespace PhrazorApp.Components.Shared
{
    /// <summary>
    /// CSVアップロードダイアログの共通基底クラス
    /// </summary>
    /// <typeparam name="TItem">アップロード結果の要素型</typeparam>
    public abstract class DialogCsvUploadBase<TItem> : ComponentBase
    {
        /// <summary>
        /// アップロード完了時に呼び出されるコールバック
        /// </summary>
        [Parameter]
        public EventCallback<List<TItem>> OnUploadCompleted { get; set; }

        /// <summary>
        /// アップロード完了を通知
        /// </summary>
        protected async Task RaiseCompletedAsync(List<TItem> items)
        {
            if (OnUploadCompleted.HasDelegate)
                await OnUploadCompleted.InvokeAsync(items);
        }
    }
}
