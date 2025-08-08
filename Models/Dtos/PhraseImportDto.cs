using CsvHelper.Configuration.Attributes;

namespace PhrazorApp.Models.Dtos
{
    public class PhraseImportDto
    {
        [Index(0)]
        public string Phrase { get; set; } = string.Empty;

        [Index(1)]
        public string Meaning { get; set; } = string.Empty;
    }
}
