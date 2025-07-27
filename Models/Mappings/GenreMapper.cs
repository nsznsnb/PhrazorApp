using PhrazorApp.Data.Entities;
using PhrazorApp.Models.ViewModels;

public static class GenreMapper
{
    /// <summary>
    /// MGenre �G���e�B�e�B�� GenreModel �ɕϊ����܂��B
    /// </summary>
    public static GenreModel ToModel(this MGenre entity)
    {
        return new GenreModel
        {
            Id = entity.GenreId,
            Name = entity.GenreName,
            SubGenres = entity.MSubGenres?
                .Select(x => new SubGenreModel
                {
                    Id = x.SubGenreId,
                    Name = x.SubGenreName,
                    SortOrder = x.SortOrder
                })
                .OrderBy(x => x.SortOrder)
                .ToList()
        };
    }

    /// <summary>
    /// GenreModel �� MGenre �G���e�B�e�B�ɕϊ����܂��B
    /// </summary>
    public static MGenre ToEntity(this GenreModel model, string? userId, DateTime now)
    {
        return new MGenre
        {
            GenreId = model.Id,
            GenreName = model.Name,
            CreatedAt = now,
            UpdatedAt = now,
            UserId = userId
        };
    }

    /// <summary>
    /// GenreModel �Ɋ�Â��āA�T�u�W�������� MSubGenre �G���e�B�e�B�̃��X�g���쐬���܂��B
    /// </summary>
    public static List<MSubGenre>? ToSubGenreEntities(this GenreModel model, string? userId, DateTime now)
    {
        return model.SubGenres?.Select(x => new MSubGenre
        {
            SubGenreId = x.Id,
            GenreId = model.Id,
            SubGenreName = x.Name,
            SortOrder = x.SortOrder,
            CreatedAt = now,
            UpdatedAt = now,
            UserId = userId
        }).ToList();
    }
}