using System.Runtime.CompilerServices;

namespace PhrazorApp.Extensions
{
    public static class LoggerExtensions
    {
        /// <summary>
        /// インフォメーションログ出力：クラス・メソッド名付きログ出力
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="eventId">イベントId</param>
        /// <param name="ex">エラー情報</param>
        /// <param name="message">エラーメッセージ概要</param>
        /// <param name="args"></param>
        /// <param name="memberName"></param>
        /// <param name="filePath"></param>
        public static void LogInformationWithContext(
            this ILogger logger,
            EventId eventId,
            string message,
            object[] args = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            var className = Path.GetFileNameWithoutExtension(filePath);
            var prefix = $"[{className}.{memberName}]";
            logger.LogInformation(eventId, prefix + message, args ?? Array.Empty<object>());
        }

        /// <summary>
        /// 警告ログ出力：クラス・メソッド名付きログ出力
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="eventId">イベントId</param>
        /// <param name="message">エラーメッセージ概要</param>
        /// <param name="args"></param>
        /// <param name="memberName"></param>
        /// <param name="filePath"></param>
        public static void LogWarningWithContext(
            this ILogger logger,
            EventId eventId,
            string message,
            object[] args = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            var className = Path.GetFileNameWithoutExtension(filePath);
            var prefix = $"[{className}.{memberName}]";
            logger.LogWarning(eventId,  prefix + message, args ?? Array.Empty<object>());
        }

        /// <summary>
        /// エラーログ出力：クラス・メソッド名付きログ出力
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="eventId">イベントId</param>
        /// <param name="message">エラーメッセージ概要</param>
        /// <param name="args"></param>
        /// <param name="memberName"></param>
        /// <param name="filePath"></param>
        public static void LogErrorWithContext(
            this ILogger logger,
            EventId eventId,
            string message,
            object[] args = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            var className = Path.GetFileNameWithoutExtension(filePath);
            var prefix = $"[{className}.{memberName}]";
            logger.LogError(eventId, prefix + message, args ?? Array.Empty<object>());
        }

        /// <summary>
        /// エラーログ出力：クラス・メソッド名付きログ出力
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="eventId">イベントId</param>
        /// <param name="ex">エラー情報</param>
        /// <param name="message">エラーメッセージ概要</param>
        /// <param name="args"></param>
        /// <param name="memberName"></param>
        /// <param name="filePath"></param>
        public static void LogErrorWithContext(
            this ILogger logger,
            EventId eventId,
            Exception ex,
            string message,
            object[] args = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "")
        {
            var className = Path.GetFileNameWithoutExtension(filePath);
            var prefix = $"[{className}.{memberName}]";
            logger.LogError(eventId, ex, prefix + message, args ?? Array.Empty<object>());
        }
    }
}
