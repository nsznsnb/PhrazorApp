using Microsoft.AspNetCore.Components;
using PhrazorApp.Commons.Results;

namespace PhrazorApp.Components.Shared.Dialogs
{
    /// <summary>CSVアップロードの共通基底</summary>
    public abstract class DialogCsvUploadBase<TItem> : ComponentBase
    {
        /// <summary>アップロード完了（UI側へ通知）</summary>
        [Parameter] public EventCallback<List<TItem>> OnUploadCompleted { get; set; }

        /// <summary>
        /// 任意：読み込んだデータをどう処理するか（DB登録など）を呼び出し元が注入。
        /// 成功/失敗メッセージは ServiceResult に載せて返す。
        /// </summary>
        [Parameter] public Func<List<TItem>, Task<ServiceResult<NoContent>>>? ProcessAsync { get; set; }

        // UIカスタマイズ
        [Parameter] public string DialogTitle { get; set; } = "CSV読込";
        [Parameter] public string ExecuteButtonText { get; set; } = "読込";
        [Parameter] public string Accept { get; set; } = ".csv";
        [Parameter] public int MaxSizeMB { get; set; } = 10;

        protected async Task RaiseCompletedAsync(List<TItem> items)
        {
            if (OnUploadCompleted.HasDelegate)
                await OnUploadCompleted.InvokeAsync(items);
        }
    }
}
