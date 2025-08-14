using MudBlazor;
using PhrazorApp.Extensions;          // ToSeverity()
using PhrazorApp.Commons;             // OverlayScope
using PhrazorApp.Commons.Results;     // ServiceResult<T>, Unit

namespace PhrazorApp.Components.UiSupport
{
    /// <summary>
    /// UI操作ランナー（キャンセルなし）
    /// 公開APIは4つのみ：
    /// 1) ReadAsync                 … 読込（オーバーレイ無し）
    /// 2) ReadWithOverlayAsync      … 読込（BodyOnly オーバーレイ）
    /// 3) WriteAsync<T>             … 書き込み（常に Global オーバーレイ）
    /// 4) WriteThenReloadAsync<TL,TO> … 書き込み → 再読込（常に Global オーバーレイ）
    ///    ※ 書き込み結果の型 TO は任意（必要がなければ Unit を使う）
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

        public string DefaultMessage { get; set; } = "処理中です…";
        public bool DefaultIndeterminate { get; set; } = true;

        // 1) 読込（オーバーレイ無し）
        public async Task<T?> ReadAsync<T>(Func<Task<ServiceResult<T>>> loadData)
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

        // 2) 読込（BodyOnly オーバーレイ）
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

        // 3) 書き込み（常に Global オーバーレイ）
        // 戻り値不要な書込みは Unit を使う: Func<Task<ServiceResult<Unit>>>
        public async Task<ServiceResult<T>> WriteAsync<T>(
            Func<Task<ServiceResult<T>>> operation,
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
                return ServiceResult.Error<T>("処理に失敗しました。");
            }
            finally
            {
                _loading.Hide();
            }
        }

        // 4) 書き込み → 再読込（常に Global オーバーレイ）
        // 書き込み結果 TO は使わない場合でも Unit でOK。再読込の結果 TL を返す。
        public async Task<TL?> WriteThenReloadAsync<TL, TO>(
            Func<Task<ServiceResult<TO>>> operation,
            Func<Task<ServiceResult<TL>>> reloadData,
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
