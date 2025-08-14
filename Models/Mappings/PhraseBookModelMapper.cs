using System.Linq.Expressions;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Models.Mappings
{
    public static class PhraseBookModelMapper
    {
        // フレーズ帳一覧（件数付き）に直接投影
        // ※ MPhraseBook に MPhraseBookItems ナビゲーションがある前提
        public static readonly Expression<Func<MPhraseBook, PhraseBookListItemModel>> ListProjection
            = b => new PhraseBookListItemModel
            {
                Id = b.PhraseBookId,
                Name = b.PhraseBookName,
                Count = b.MPhraseBookItems.Count()
            };

        // フレーズ（DPhrase）→ 一覧行
        public static readonly Expression<Func<DPhrase, PhraseBookItemModel>> ItemProjection
            = p => new PhraseBookItemModel
            {
                Id = p.PhraseId,
                English = p.Phrase ?? string.Empty,
                Japanese = p.Meaning ?? string.Empty,
                CreatedAt = p.CreatedAt
            };

        // IQueryable 拡張
        public static IQueryable<PhraseBookListItemModel> SelectList(this IQueryable<MPhraseBook> q)
            => q.Select(ListProjection);

        public static IQueryable<PhraseBookItemModel> SelectItems(this IQueryable<DPhrase> q)
            => q.Select(ItemProjection);
    }
}
