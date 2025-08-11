using PhrazorApp.Data.Entities;

namespace PhrazorApp.Models.Mappings
{
    public static class GenreModelMapper
    {
        public static GenreModel ToModel(this MGenre e) => new()
        {
            Id = e.GenreId,
            Name = e.GenreName,
            SubGenres = e.MSubGenres?
                .Select(s => new SubGenreModel
                {
                    Id = s.SubGenreId,
                    Name = s.SubGenreName,
                    OrderNo = s.OrderNo
                })
                .OrderBy(x => x.OrderNo)
                .ToList() ?? new()
        };

        public static MGenre ToEntity(this GenreModel m, string userId)
        {
            var g = new MGenre
            {
                GenreId = m.Id,
                GenreName = m.Name,
                UserId = userId,
                MSubGenres = new List<MSubGenre>()
            };

            if (m.SubGenres?.Count > 0)
            {
                g.MSubGenres = m.SubGenres.Select(x => new MSubGenre
                {
                    SubGenreId = x.Id,
                    GenreId = m.Id,
                    SubGenreName = x.Name,
                    OrderNo = x.OrderNo,
                    UserId = userId
                }).ToList();
            }
            return g;
        }
    }

}
