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

    /// <summary>生成用のファクトリ（ジェネリックのみ）</summary>
    public static class ServiceResult
    {
        public static ServiceResult<T> Success<T>(T data, string? message = null)
            => new() { Status = ServiceStatus.Success, Data = data, Message = message };

        public static ServiceResult<T> Warning<T>(T data, string? message = null)
            => new() { Status = ServiceStatus.Warning, Data = data, Message = message };

        public static ServiceResult<T> Error<T>(string message)
            => new() { Status = ServiceStatus.Error, Data = default, Message = message };
    }

}
