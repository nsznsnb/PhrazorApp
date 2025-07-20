using PhrazorApp.Common;

namespace PhrazorApp.Options
{
    /// <summary>
    /// メール送信設定
    /// </summary>
    public class ResendOptions
    {
        public string ApiKey { get; set; } = "";
        public string FromEmail { get; set; } = "";
        public string FromName { get; set; } = ComDefine.APP_NAME;
    }
}
