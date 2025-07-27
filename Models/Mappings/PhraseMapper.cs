using PhrazorApp.Data.Entities;
using PhrazorApp.Models.ViewModels;

namespace PhrazorApp.Models.Mappings
{
    public static class PhraseMapper
    {
        /// <summary>
        /// DPhrase → PhraseModel（エンティティ → モデル）
        /// </summary>
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

        /// <summary>
        /// PhraseModel → DPhrase（モデル → エンティティ）
        /// ※ PhraseImage は別で処理
        /// </summary>
        public static DPhrase ToPhraseEntity(this PhraseModel model, string? userId, DateTime now)
        {
            return new DPhrase
            {
                PhraseId = model.Id,
                Phrase = model.Phrase,
                Meaning = model.Meaning,
                Note = model.Note,
                UserId = userId,
                CreatedAt = now,
                UpdatedAt = now,
            };
        }

        /// <summary>
        /// PhraseModel → DPhraseImage（画像エンティティ）
        /// </summary>
        public static DPhraseImage ToImageEntity(this PhraseModel model, DateTime now)
        {
            return new DPhraseImage
            {
                Url = model.ImageUrl,
                PhraseId = model.Id,
                CreatedAt = now,
                UpdatedAt = now,
                UploadAt = now
            };
        }

        /// <summary>
        /// DropItemModel → MPhraseGenre リストへ変換
        /// </summary>
        public static List<MPhraseGenre> ToPhraseGenreEntities(
            this List<DropItemModel> dropItems,
            Guid phraseId,
            DateTime now)
        {
            return dropItems
                .Where(item => item.DropTarget == DropItemType.Target)
                .Select(item => new MPhraseGenre
                {
                    PhraseId = phraseId,
                    GenreId = item.GenreId,
                    SubGenreId = item.SubGenreId,
                    CreatedAt = now,
                    UpdatedAt = now
                })
                .ToList();
        }

        /// <summary>
        /// MGenre リストと MPhraseGenre を元に DropItemModel リストを作成
        /// </summary>
        public static List<DropItemModel> ToDropItemModelList(
            this List<MGenre> allGenres,
            List<MPhraseGenre> selectedGenres)
        {
            var result = new List<DropItemModel>();

            foreach (var genre in allGenres)
            {
                foreach (var subGenre in genre.MSubGenres)
                {
                    var isSelected = selectedGenres.Any(x => x.SubGenreId == subGenre.SubGenreId);
                    result.Add(new DropItemModel
                    {
                        Id = subGenre.SubGenreId,
                        Name = subGenre.SubGenreName,
                        Group = genre.GenreName,
                        DropTarget = isSelected ? DropItemType.Target : DropItemType.UnAssigned,
                        GenreId = genre.GenreId,
                        SubGenreId = subGenre.SubGenreId
                    });
                }
            }

            return result;
        }
    }
}
