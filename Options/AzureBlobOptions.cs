namespace PhrazorApp.Options
{
    /// <summary>
    /// AzureBlob設定
    /// </summary>
    public class AzureBlobOptions
    {
        public string ConnectionString { get; set; } = "";
        public string ContainerName { get; set; } = "";
    }
}
