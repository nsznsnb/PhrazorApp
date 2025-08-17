using CsvHelper.Configuration.Attributes;

namespace PhrazorApp.Models.Dtos
{
    public sealed class ProverbImportDto
    {
        [Index(0)] public string Text { get; set; } = string.Empty;
        [Index(1)] public string Meaning { get; set; } = string.Empty;
        [Index(2)] public string Author { get; set; } = string.Empty;
    }
}
