using PhrazorApp.Data.Entities;

namespace PhrazorApp.Models.Mappings
{
    public static class DropItemModelMapper
    {
        public static List<DropItemModel> ToDropItemModelList(this IEnumerable<MGenre> genres)
            => genres?
                .SelectMany(g => (g.MSubGenres ?? Enumerable.Empty<MSubGenre>()).Select(s => new DropItemModel
                {
                    Key1 = g.GenreId,
                    Key2 = s.SubGenreId,
                    Name = $"{g.GenreName} / {s.SubGenreName}",
                    DropTarget = DropItemType.UnAssigned
                }))
                .ToList() ?? new();

        public static List<DropItemModel> ToDropItemModels(this IEnumerable<MPhraseGenre> list)
            => list?.Select(x => new DropItemModel
            {
            Key1 = x.GenreId,
            Key2 = x.SubGenreId,
            DropTarget = DropItemType.Target
            }).ToList() ?? new();
    }
}
