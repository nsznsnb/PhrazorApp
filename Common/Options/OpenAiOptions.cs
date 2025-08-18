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

    /// <summary>
    /// チャットモデル
    /// </summary>
    public string ChatModel { get; set; } = "gpt-4o-mini";

    /// <summary>
    /// 画像生成モデル
    /// </summary>
    public string ImageModel { get; set; } = "gpt-image-1";
}
