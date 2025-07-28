using PhrazorApp.Data.Entities;
using PhrazorApp.Models;

public static class ZModelMapper
{

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

    public static MGenre ToEntity(this GenreModel model)
    {
        var genre = new MGenre
        {
            GenreId = model.Id,
            GenreName = model.Name,
        };

        if (model.SubGenres != null)
        {

            genre.MSubGenres = model.SubGenres.Select(x => new MSubGenre
            {
                SubGenreId = x.Id,
                GenreId = model.Id,
                SubGenreName = x.Name,
                SortOrder = x.SortOrder,
            }).ToList();
        }
        return genre;
    }



    public static List<DropItemModel> ToDropItemModelList(this IEnumerable<MGenre> genres)
    {
        return genres
            .SelectMany(large => large.MSubGenres.Select(small => new DropItemModel
            {
                Key1 = large.GenreId,
                Key2 = small.GenreId,
                Name = $"{large.GenreName} / {small.SubGenreName}",
                DropTarget = DropItemType.UnAssigned,
            }))
            .ToList();
    }

    public static PhraseModel ToPhraseModel(this DPhrase entity)
    {
        return new PhraseModel
        {
            Id = entity.PhraseId,
            Phrase = entity.Phrase ?? string.Empty,
            Meaning = entity.Meaning ?? string.Empty,
            Note = entity.Note ?? string.Empty,
            ImageUrl = entity.DPhraseImage?.Url ?? string.Empty,
        };
    }

    public static DPhrase ToPhraseEntity(this PhraseModel model)
    {
        return new DPhrase
        {
            PhraseId = model.Id,
            Phrase = model.Phrase,
            Meaning = model.Meaning,
            Note = model.Note,
        };
    }

    public static DPhraseImage ToImageEntity(this PhraseModel model, DateTime now)
    {
        return new DPhraseImage
        {
            Url = model.ImageUrl,
            PhraseId = model.Id,
            UploadAt = now
        };
    }

    public static List<DropItemModel> ToDropItemModels(this IEnumerable<MPhraseGenre> genres)
    {
        return genres.Select(x => new DropItemModel
        {
            Key1 = x.GenreId,
            Key2 = x.SubGenreId,
            DropTarget = DropItemType.Target
        }).ToList(); 
    }

    public static List<MPhraseGenre> ToPhraseGenreEntities(this PhraseModel model)
    {
        return model.SelectedDropItems.Select(x => new MPhraseGenre
        {
            GenreId = x.Key1,
            SubGenreId = x.Key2 ?? Guid.NewGuid(),
            PhraseId = model.Id,
        }).ToList();
    }

}