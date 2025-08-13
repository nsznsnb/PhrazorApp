using MudBlazor;
using PhrazorApp.Extensions;     // ToSeverity()
using PhrazorApp.Commons;        // ServiceResult, OverlayScope

namespace PhrazorApp.Components.UiSupport
{
    /// <summary>
    /// UI操作ランナー（キャンセルなし・公開APIは4つ）
    /// 1) ReadAsync                   … 読込（ローディングなし）
    /// 2) ReadWithOverlayAsync        … 読込（BodyOnly オーバーレイ）
    /// 3) WriteAsync                  … 登録・削除（BodyOnly オーバーレイ）
    /// 4) WriteThenReloadAsync        … 登録・削除 → 再読込（Global オーバーレイ）
    /// </summary>
    public sealed class UiOperationRunner
    {
        private readonly ISnackbar _snackbar;
        private readonly LoadingManager _loading;

        public UiOperationRunner(ISnackbar snackbar, LoadingManager loading)
        {
            _snackbar = snackbar;
            _loading = loading;
        }

        /// <summary>既定のローディング文言</summary>
        public string DefaultMessage { get; set; } = "処理中です…";

        /// <summary>true: 不確定進捗</summary>
        public bool DefaultIndeterminate { get; set; } = true;

        // ========================= 1) 読込（ローディングなし） =========================
        public async Task<T?> ReadNoOverlayAsync<T>(Func<Task<ServiceResult<T>>> loadData)
        {
            try
            {
                var r = await loadData();
                if (!string.IsNullOrWhiteSpace(r.Message))
                    _snackbar.Add(r.Message, r.ToSeverity());
                return r.IsSuccess ? r.Data : default;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                _snackbar.Add("処理に失敗しました。", Severity.Error);
                return default;
            }
        }

        // ========================= 2) 読込（BodyOnly オーバーレイ） =========================
        public async Task<T?> ReadWithOverlayAsync<T>(
            Func<Task<ServiceResult<T>>> loadData,
            string? message = null)
        {
            _loading.Show(message ?? DefaultMessage, indeterminate: DefaultIndeterminate, scope: OverlayScope.BodyOnly);
            try
            {
                var r = await loadData();
                if (!string.IsNullOrWhiteSpace(r.Message))
                    _snackbar.Add(r.Message, r.ToSeverity());
                return r.IsSuccess ? r.Data : default;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                _snackbar.Add("処理に失敗しました。", Severity.Error);
                return default;
            }
            finally
            {
                _loading.Hide();
            }
        }

        // ========================= 3) 登録・削除（BodyOnly オーバーレイ） =========================
        public async Task<ServiceResult> WriteAsync(
            Func<Task<ServiceResult>> operation,
            string? message = null)
        {
            _loading.Show(message ?? DefaultMessage, indeterminate: DefaultIndeterminate, scope: OverlayScope.Global);
            try
            {
                var r = await operation();
                if (!string.IsNullOrWhiteSpace(r.Message))
                    _snackbar.Add(r.Message, r.ToSeverity());
                return r;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                _snackbar.Add("処理に失敗しました。", Severity.Error);
                return ServiceResult.Failure("処理に失敗しました。");
            }
            finally
            {
                _loading.Hide();
            }
        }

        // ===== 4) 登録・削除 → 再読込（Global オーバーレイ） =====
        public async Task<T?> WriteThenReloadAsync<T>(
            Func<Task<ServiceResult>> operation,
            Func<Task<ServiceResult<T>>> reloadData,
            string? message = null)
        {
            _loading.Show(message ?? DefaultMessage, indeterminate: DefaultIndeterminate, scope: OverlayScope.Global);
            try
            {
                var op = await operation();

                if (!string.IsNullOrWhiteSpace(op.Message))
                    _snackbar.Add(op.Message, op.ToSeverity());

                if (!op.IsSuccess) return default;

                var load = await reloadData();

                if (!string.IsNullOrWhiteSpace(load.Message))
                    _snackbar.Add(load.Message, load.ToSeverity());

                return load.IsSuccess ? load.Data : default;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                _snackbar.Add("処理に失敗しました。", Severity.Error);
                return default;
            }
            finally
            {
                _loading.Hide();
            }
        }
    }
}
