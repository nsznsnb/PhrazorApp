namespace PhrazorApp.Common;
/// <summary>
/// OpenAi設定
/// </summary>
public class OpenAiOptions
{
    /// <summary>
    /// APIキー
    /// </summary>
    public string ApiKey { get; set; } = "";
    /// <summary>
    /// 画像サイズ
    /// </summary>
    public string ImageSize { get; set; } = "256x256";
}
