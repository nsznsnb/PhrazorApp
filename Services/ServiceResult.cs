using System.Buffers;

namespace PhrazorApp.Services
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


    public interface IServiceResult {
        /// <summary>
        /// サービス結果ステータス
        /// </summary>
        public ServiceStatus Status { get; set; }
        /// <summary>
        /// 処理結果メッセージ
        /// </summary>
        public string? Message { get; set; }


        /// <summary>
        /// 成功状態か
        /// </summary>
        public bool IsSuccess => Status == ServiceStatus.Success;
        /// <summary>
        /// 警告状態か
        /// </summary>
        public bool IsWarning => Status == ServiceStatus.Warning;
        /// <summary>
        /// エラー状態か
        /// </summary>
        public bool IsError => Status == ServiceStatus.Error;
    }

    /// <summary>
    /// サービス結果クラス(ジェネリック版)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServiceResult<T> : IServiceResult
    {
        /// <summary>
        /// サービス結果ステータス
        /// </summary>
        public ServiceStatus Status { get; set; }
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
        /// <param name="data">結果データ</param>
        /// <param name="message">処理結果メッセージ</param>
        /// <returns></returns>
        public static ServiceResult<T> Success(T data, string? message = null)
        {
            return new ServiceResult<T> { Status = ServiceStatus.Success, Data  = data, Message = message };
        }


        /// <summary>
        /// 警告あり
        /// </summary>
        /// <param name="data">結果データ</param>
        /// <param name="message">処理結果メッセージ</param>
        /// <returns></returns>
        public static ServiceResult<T> Warning(T data, string? message = null)
        {
            return new ServiceResult<T> { Status = ServiceStatus.Success, Data = data, Message = message };
        }


        /// <summary>
        /// 処理失敗
        /// </summary>
        /// <param name="message">処理結果メッセージ</param>
        /// <returns></returns>
        public static ServiceResult<T> Failure(string message)
        {
            return new ServiceResult<T> { Status = ServiceStatus.Error , Message = message };
        }
    }

    /// <summary>
    /// サービス結果クラス(非ジェネリック版)
    /// </summary>
    public class ServiceResult : IServiceResult
    {
        /// <summary>
        /// サービス結果ステータス
        /// </summary>
        public ServiceStatus Status { get; set; }
        /// <summary>
        /// 処理結果メッセージ
        /// </summary>
        public string? Message { get; set; }



        /// <summary>
        /// 処理成功
        /// </summary>
        /// <param name="message">処理結果メッセージ</param>
        /// <returns></returns>
        public static ServiceResult Success(string? message = null)
        {
            return new ServiceResult { Status = ServiceStatus.Success, Message = message };
        }


        /// <summary>
        /// 警告あり
        /// </summary>
        /// <param name="message">処理結果メッセージ</param>
        /// <returns></returns>
        public static ServiceResult Warning(string? message = null)
        {
            return new ServiceResult { Status = ServiceStatus.Success, Message = message };
        }


        /// <summary>
        /// 処理失敗
        /// </summary>
        /// <param name="message">処理結果メッセージ</param>
        /// <returns></returns>
        public static ServiceResult Failure(string message)
        {
            return new ServiceResult { Status = ServiceStatus.Error, Message = message };
        }
    }
}
