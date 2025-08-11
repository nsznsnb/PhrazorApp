using MudBlazor;
using PhrazorApp.Extensions;

namespace PhrazorApp.Components.UiSupport
{

    /// <summary>
    /// UI 操作の共通ラッパー。ローディング表示（遅延表示対応）・キャンセル・Snackbar を一元化。
    /// - Message が null/空のときは Snackbar を出さない
    /// - 例外/キャンセルはここで一元処理
    /// - showAfter: ローディングカードの「表示までの遅延」。短時間処理のちらつきを抑える
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

        // ======================== 非ジェネリック（作成/更新/削除など） ========================
        public async Task<ServiceResult> RunAsync(
            Func<CancellationToken, Task<ServiceResult>> action,
            string runningMessage,
            TimeSpan? timeout = null,
            bool showCancel = true,
            TimeSpan? showAfter = null,                     // ★ 追加: 遅延表示（既定: 150ms）
            CancellationToken ct = default)
        {
            var delay = showAfter ?? TimeSpan.FromMilliseconds(150);
            using var timeoutCts = timeout is null ? new CancellationTokenSource()
                                                   : new CancellationTokenSource(timeout.Value);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, ct);

            var opTask = action(linked.Token);

            // 遅延表示タスク：150ms 経過しても未完了ならカードを表示
            var shown = false;
            var showTask = Task.Run(async () =>
            {
                try { await Task.Delay(delay, linked.Token); } catch { /* 取消時 */ }
                if (!opTask.IsCompleted && !linked.IsCancellationRequested)
                {
                    _loading.Show(runningMessage, showCancel: showCancel, onCancel: () => linked.Cancel());
                    shown = true;
                }
            });

            try
            {
                var result = await opTask;

                if (!string.IsNullOrWhiteSpace(result.Message))
                    _snackbar.Add(result.Message, result.ToSeverity());

                return result;
            }
            catch (OperationCanceledException)
            {
                _snackbar.Add("キャンセルしました。", Severity.Info);
                return ServiceResult.Warning("キャンセルしました。");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                _snackbar.Add("処理に失敗しました。", Severity.Error);
                return ServiceResult.Failure("処理に失敗しました。");
            }
            finally
            {
                // showTask の完了を待ってから、表示していた場合のみ閉じる
                try { await showTask; } catch { /* no-op */ }
                if (shown) _loading.Hide();
            }
        }

        // ======================== ジェネリック（読み取り・戻り値あり） ========================
        public async Task<ServiceResult<T>> RunAsync<T>(
            Func<CancellationToken, Task<ServiceResult<T>>> action,
            string runningMessage,
            TimeSpan? timeout = null,
            bool showCancel = true,
            TimeSpan? showAfter = null,                     // ★ 追加
            CancellationToken ct = default)
        {
            var delay = showAfter ?? TimeSpan.FromMilliseconds(150);
            using var timeoutCts = timeout is null ? new CancellationTokenSource()
                                                   : new CancellationTokenSource(timeout.Value);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, ct);

            var opTask = action(linked.Token);

            var shown = false;
            var showTask = Task.Run(async () =>
            {
                try { await Task.Delay(delay, linked.Token); } catch { /* 取消時 */ }
                if (!opTask.IsCompleted && !linked.IsCancellationRequested)
                {
                    _loading.Show(runningMessage, showCancel: showCancel, onCancel: () => linked.Cancel());
                    shown = true;
                }
            });

            try
            {
                var result = await opTask;

                if (!string.IsNullOrWhiteSpace(result.Message))
                    _snackbar.Add(result.Message, result.ToSeverity());

                return result;
            }
            catch (OperationCanceledException)
            {
                _snackbar.Add("キャンセルしました。", Severity.Info);
                return ServiceResult.Warning<T>(default!, "キャンセルしました。");
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex);
                _snackbar.Add("処理に失敗しました。", Severity.Error);
                return ServiceResult.Failure<T>("処理に失敗しました。");
            }
            finally
            {
                try { await showTask; } catch { /* no-op */ }
                if (shown) _loading.Hide();
            }
        }

        // ======================== 糖衣：読み取りだけ（T をそのまま欲しい） ========================
        public async Task<T?> RunLoadOnlyAsync<T>(
            Func<CancellationToken, Task<ServiceResult<T>>> loadData,
            string runningMessage,
            TimeSpan? timeout = null,
            bool showCancel = false,
            TimeSpan? showAfter = null,                     // ★ 追加
            CancellationToken ct = default)
        {
            var r = await RunAsync(loadData, runningMessage, timeout, showCancel, showAfter, ct);
            return r.IsSuccess ? r.Data : default;
        }

        // ======================== 糖衣：操作 → 成功したら読み直す ========================
        public async Task<T?> RunThenLoadAsync<T>(
            Func<CancellationToken, Task<ServiceResult>> doOperation,
            Func<CancellationToken, Task<ServiceResult<T>>> loadData,
            string runningMessage,
            TimeSpan? timeout = null,
            bool showCancel = true,
            TimeSpan? showAfter = null,                     // ★ 追加
            CancellationToken ct = default)
        {
            var delay = showAfter ?? TimeSpan.FromMilliseconds(0);
            using var timeoutCts = timeout is null ? new CancellationTokenSource()
                                                   : new CancellationTokenSource(timeout.Value);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, ct);

            var shown = false;
            var opTask = doOperation(linked.Token);

            var showTask = Task.Run(async () =>
            {
                try { await Task.Delay(delay, linked.Token); } catch { /* 取消時 */ }
                if (!opTask.IsCompleted && !linked.IsCancellationRequested)
                {
                    _loading.Show(runningMessage, showCancel: showCancel, onCancel: () => linked.Cancel());
                    shown = true;
                }
            });

            try
            {
                var op = await opTask;

                if (!string.IsNullOrWhiteSpace(op.Message))
                    _snackbar.Add(op.Message, op.ToSeverity());
                if (!op.IsSuccess) return default;

                var load = await loadData(linked.Token);
                if (!string.IsNullOrWhiteSpace(load.Message))
                    _snackbar.Add(load.Message, load.ToSeverity());

                return load.IsSuccess ? load.Data : default;
            }
            catch (OperationCanceledException)
            {
                _snackbar.Add("キャンセルしました。", Severity.Info);
                return default;
            }
            catch
            {
                _snackbar.Add("処理に失敗しました。", Severity.Error);
                return default;
            }
            finally
            {
                try { await showTask; } catch { /* no-op */ }
                if (shown) _loading.Hide();
            }
        }

    }
}