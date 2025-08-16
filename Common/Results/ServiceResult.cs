
namespace PhrazorApp.Common.Results
{
    public enum ServiceStatus
    {
        /// <summary>
        /// 処理成功
        /// </summary>
        Success,
        /// <summary>
        /// 警告
        /// </summary>
        Warning,
        /// <summary>
        /// 処理失敗
        /// </summary>
        Error
    }

    /// <summary>結果 + メッセージ + ステータス（ジェネリックのみ）</summary>
    public sealed class ServiceResult<T>
    {
        public ServiceStatus Status { get; init; }
        public string? Message { get; init; }
        public T? Data { get; init; }

        public bool IsSuccess => Status == ServiceStatus.Success;
        public bool IsWarning => Status == ServiceStatus.Warning;
        public bool IsError => Status == ServiceStatus.Error;
    }

    public static class ServiceResult
    {
        public static ServiceResult<T> Success<T>(T data, string? message = null)
            => new() { Status = ServiceStatus.Success, Data = data, Message = message };

        public static ServiceResult<T> Warning<T>(T data, string? message = null)
            => new() { Status = ServiceStatus.Warning, Data = data, Message = message };

        public static ServiceResult<T> Error<T>(string message)
            => new() { Status = ServiceStatus.Error, Data = default, Message = message };

        public static class None
        {
            public static ServiceResult<Unit> Success(string? message = null)
                => ServiceResult.Success(Unit.Value, message);

            public static ServiceResult<Unit> Warning(string? message = null)
                => ServiceResult.Warning(Unit.Value, message);

            public static ServiceResult<Unit> Error(string message)
                => ServiceResult.Error<Unit>(message);
        }
    }

}
