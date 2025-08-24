namespace PhrazorApp.Common;

public sealed class AzureStorageOptions
{
    public string ConnectionString { get; set; } = "";
    public ContainersOptions Containers { get; set; } = new();

    public sealed class ContainersOptions
    {
        public string Images { get; set; } = "phraseimage";
        public string DataProtection { get; set; } = "dataprotection";
    }
}