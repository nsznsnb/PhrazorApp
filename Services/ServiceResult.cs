namespace PhrazorApp.Services
{
    /// <summary>
    /// サービス結果クラス(ジェネリック版)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServiceResult<T>
    {
        /// <summary>
        /// 処理に成功したか
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 処理結果メッセージ
        /// </summary>
        public string? Message { get; set; }
        /// <summary>
        /// サービス処理データ
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// 処理成功
        /// </summary>
        /// <param name="data"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ServiceResult<T> Success(T data, string? message = null)
        {
            return new ServiceResult<T> { IsSuccess = true, Data  = data, Message = message };
        }

        /// <summary>
        /// 処理失敗
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ServiceResult<T> Failure(string message)
        {
            return new ServiceResult<T> { IsSuccess = false, Message = message };
        }
    }

    /// <summary>
    /// サービス結果クラス(非ジェネリック版)
    /// </summary>
    public class ServiceResult
    {
        /// <summary>
        /// 処理に成功したか
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 処理結果メッセージ
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// 処理成功
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static ServiceResult Success(string? message = null)
        {
            return new ServiceResult { IsSuccess = true, Message = message };
        }

        /// <summary>
        /// 処理失敗
        /// </summary>
        /// <param name="message"></param>
        public static ServiceResult Failure(string message)
        {
            return new ServiceResult { IsSuccess = false, Message = message };
        }
    }
}
