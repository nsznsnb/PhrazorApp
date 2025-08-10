using PhrazorApp.Data.Entities;

namespace PhrazorApp.Models.Mappings
{
    public static class PhraseModelMapper
    {
        public static PhraseModel ToModel(this DPhrase e) => new()
        {
            Id = e.PhraseId,
            Phrase = e.Phrase ?? string.Empty,
            Meaning = e.Meaning ?? string.Empty,
            Note = e.Note ?? string.Empty,
            ImageUrl = e.DPhraseImage?.Url ?? string.Empty,
        };

        public static DPhrase ToEntity(this PhraseModel m, string userId) => new()
        {
            PhraseId = m.Id,  // UIで発番されたGuidが入る
            Phrase = m.Phrase,
            Meaning = m.Meaning,
            Note = m.Note,
            UserId = userId
        };

        public static DPhraseImage ToImageEntity(this PhraseModel m, DateTime uploadAtUtc) => new()
        {
            PhraseImageId = Guid.NewGuid(), // ★必須（ValueGeneratedNever）
            PhraseId = m.Id,
            Url = m.ImageUrl,
            UploadAt = uploadAtUtc
        };

        public static List<MPhraseGenre> ToPhraseGenreEntities(this PhraseModel m)
        {
            if (m.SelectedDropItems is null) return new();
            return m.SelectedDropItems
                .Where(x => x.Key2.HasValue)
                .DistinctBy(x => (x.Key1, x.Key2!.Value)) // ★複合PK重複回避
                .Select(x => new MPhraseGenre
                {
                    PhraseId = m.Id,
                    GenreId = x.Key1,
                    SubGenreId = x.Key2!.Value
                })
                .ToList();
        }




    }
}
