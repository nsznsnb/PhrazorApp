using PhrazorApp.Data.Entities;

namespace PhrazorApp.Models.Mappings
{
    public static class PhraseGenreMapper
    {
        public static List<DropItemModel> ToDropItemModels(this IEnumerable<MPhraseGenre> list)
            => list?.Select(x => new DropItemModel
            {
                Key1 = x.GenreId,
                Key2 = x.SubGenreId,
                DropTarget = DropItemType.Target
            }).ToList() ?? new();

        public static List<MPhraseGenre> ToPhraseGenreEntities(this PhraseModel m)
        {
            if (m.SelectedDropItems is null) return new();

            // null SubGenre はスキップ（仕様に合わせて変更可）
            return m.SelectedDropItems
                .Where(x => x.Key2.HasValue)
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
