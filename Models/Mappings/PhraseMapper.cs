using PhrazorApp.Data.Entities;

namespace PhrazorApp.Models.Mappings
{
    public static class PhraseMapper
    {
        public static PhraseModel ToModel(this DPhrase e) => new()
        {
            Id = e.PhraseId,
            Phrase = e.Phrase ?? string.Empty,
            Meaning = e.Meaning ?? string.Empty,
            Note = e.Note ?? string.Empty,
            ImageUrl = e.DPhraseImage?.Url ?? string.Empty,
        };

        public static DPhrase ToEntity(this PhraseModel m) => new()
        {
            PhraseId = m.Id,
            Phrase = m.Phrase,
            Meaning = m.Meaning,
            Note = m.Note
        };

        public static DPhraseImage ToImageEntity(this PhraseModel m, DateTime uploadAtUtc) => new()
        {
            PhraseId = m.Id,
            Url = m.ImageUrl,
            UploadAt = uploadAtUtc
        };
    }
}
