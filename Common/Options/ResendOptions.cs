namespace PhrazorApp.Common;

/// <summary>
/// メール送信設定
/// </summary>
public class ResendOptions
{
    /// <summary>
    /// APIキー
    /// </summary>
    public string ApiKey { get; set; } = "";
    /// <summary>
    /// 送信元メールアドレス
    /// </summary>
    public string FromEmail { get; set; } = "";
    /// <summary>
    /// 送信者名
    /// </summary>
    public string FromName { get; set; } = AppConstants.APP_NAME;
}
