using MudBlazor;
using System.Buffers;

namespace PhrazorApp.Commons.Results
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

    public class ServiceResult
    {
        public ServiceStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;

        public bool IsSuccess => Status == ServiceStatus.Success;
        public bool IsWarning => Status == ServiceStatus.Warning;
        public bool IsError => Status == ServiceStatus.Error;

        // ===== 非ジェネリック版 =====
        public static ServiceResult Success(string? message = null)
            => new ServiceResult { Status = ServiceStatus.Success, Message = message };

        public static ServiceResult Warning(string? message = null)
            => new ServiceResult { Status = ServiceStatus.Warning, Message = message };

        public static ServiceResult Failure(string message)
            => new ServiceResult { Status = ServiceStatus.Error, Message = message };

        // ===== ジェネリック返却版 =====
        public static ServiceResult<T> Success<T>(T data, string? message = null)
            => new ServiceResult<T> { Status = ServiceStatus.Success, Data = data, Message = message };

        public static ServiceResult<T> Warning<T>(T data, string? message = null)
            => new ServiceResult<T> { Status = ServiceStatus.Warning, Data = data, Message = message };

        public static ServiceResult<T> Failure<T>(string message)
            => new ServiceResult<T> { Status = ServiceStatus.Error, Data = default, Message = message };
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; set; }

    }

}
