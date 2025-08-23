using PhrazorApp.Data.Entities;
using System.Linq;                // DistinctBy 用
using System.Linq.Expressions;

namespace PhrazorApp.Models.Mappings
{
    public static class PhraseModelMapper
    {
        // =========================
        // 編集用（Entity -> EditModel）
        // =========================
        public static PhraseEditModel ToModel(this DPhrase e) => new()
        {
            Id = e.PhraseId,
            Phrase = e.Phrase ?? string.Empty,
            Meaning = e.Meaning ?? string.Empty,
            Note = e.Note ?? string.Empty,
            ImageUrl = e.DPhraseImage?.Url ?? string.Empty,

            // ★ DPhrase に ReviewCount 列は無いので、復習ログ数から算出
            ReviewCount = e.DReviewLogs?.Count ?? 0,

            SelectedDropItems = e.MPhraseGenres?
                .OrderBy(pg => pg.MSubGenre.OrderNo)              // ThenInclude(MSubGenre)前提
                .Select(pg => new DropItemModel
                {
                    Key1 = pg.GenreId,
                    Key2 = pg.SubGenreId,
                    Name = pg.MSubGenre.SubGenreName,
                    DropTarget = DropItemType.Target
                })
                .ToList() ?? new()
        };

        public static DPhrase ToEntity(this PhraseEditModel m, string userId) => new()
        {
            PhraseId = m.Id,   // UIで発番
            Phrase = m.Phrase,
            Meaning = m.Meaning,
            Note = m.Note,
            UserId = userId
        };

        public static DPhraseImage ToImageEntity(this PhraseEditModel m, DateTime uploadAtUtc) => new()
        {
            PhraseImageId = Guid.NewGuid(), // ValueGeneratedNever
            PhraseId = m.Id,
            Url = m.ImageUrl,
            UploadAt = uploadAtUtc
        };

        public static List<MPhraseGenre> ToPhraseGenreEntities(this PhraseEditModel m)
        {
            if (m.SelectedDropItems is null) return new();
            return m.SelectedDropItems
                .Where(x => x.Key2.HasValue)
                .DistinctBy(x => (x.Key1, x.Key2!.Value)) // 複合PK重複回避
                .Select(x => new MPhraseGenre
                {
                    PhraseId = m.Id,
                    GenreId = x.Key1,
                    SubGenreId = x.Key2!.Value
                })
                .ToList();
        }

        // =========================
        // 一覧用（EF で直接投影できる式）
        // =========================
        public static readonly Expression<Func<DPhrase, PhraseListItemModel>> ListProjection
            = p => new PhraseListItemModel
            {
                Id = p.PhraseId,
                Phrase = p.Phrase ?? string.Empty,
                Meaning = p.Meaning ?? string.Empty,
                CreatedAt = p.CreatedAt,
                ReviewCount = p.DReviewLogs.Count(),

                SelectedDropItems = p.MPhraseGenres
                    .OrderBy(pg => pg.MSubGenre.OrderNo)
                    .Select(pg => new DropItemModel
                    {
                        Key1 = pg.GenreId,
                        Key2 = pg.SubGenreId,
                        Name = pg.MSubGenre.SubGenreName,              // 小分類
                        ParentName = pg.MSubGenre.Genre.GenreName,           // ★ 親（大分類）
                        DropTarget = DropItemType.Target
                    })
                    .ToList()
            };

        // IQueryable 拡張：Select だけで呼べるように
        public static IQueryable<PhraseListItemModel> SelectListModel(this IQueryable<DPhrase> q)
            => q.Select(ListProjection);
    }
}
