using PhrazorApp.Data.Entities;

namespace PhrazorApp.Models.Mappings
{
    public static class DropItemMapper
    {
        public static List<DropItemModel> ToDropItemModelList(this IEnumerable<MGenre> genres)
            => genres?.SelectMany(g => g.MSubGenres.Select(s => new DropItemModel
            {
                Key1 = g.GenreId,
                Key2 = s.SubGenreId, // ← 修正
                Name = $"{g.GenreName} / {s.SubGenreName}",
                DropTarget = DropItemType.UnAssigned
            }))
                .ToList() ?? new();
    }
}
