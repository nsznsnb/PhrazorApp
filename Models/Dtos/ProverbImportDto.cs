using CsvHelper.Configuration.Attributes;

namespace PhrazorApp.Models.Dtos
{
    public sealed class ProverbImportDto
    {
        [Name("ProverbText")] public string Text { get; set; } = string.Empty;
        [Name("Meaning")] public string? Meaning { get; set; }
        [Name("Author")] public string? Author { get; set; }
    }
}
