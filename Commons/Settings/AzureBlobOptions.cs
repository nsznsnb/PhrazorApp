namespace PhrazorApp.Commons.Settings
{
    /// <summary>
    /// AzureBlob設定
    /// </summary>
    public class AzureBlobOptions
    {
        /// <summary>
        /// 接続文字列
        /// </summary>
        public string ConnectionString { get; set; } = "";
        /// <summary>
        /// コンテナ名
        /// </summary>
        public string ContainerName { get; set; } = "";
    }
}
