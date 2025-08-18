using System.Linq.Expressions;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Models.Mappings
{
    public static class DiaryTagMapper
    {
        /// <summary>一覧投影式（クエリ内で使用）</summary>
        public static readonly Expression<Func<MDiaryTag, DiaryTagModel>> ListProjection
            = e => new DiaryTagModel
            {
                Id = e.TagId,
                Name = e.TagName,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            };

        public static DiaryTagModel ToModel(this MDiaryTag e)
            => new()
            {
                Id = e.TagId,
                Name = e.TagName,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            };

        /// <summary>新規作成用エンティティ生成（Id未指定なら採番）</summary>
        public static MDiaryTag ToEntityForCreate(this DiaryTagModel model, string userId, string normalizedName)
            => new()
            {
                TagId = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id,
                TagName = normalizedName,
                UserId = userId
                // CreatedAt/UpdatedAt は BaseRepository.Stamp で自動設定
            };

        /// <summary>更新反映（Name のみ可変）</summary>
        public static void ApplyTo(this DiaryTagModel model, MDiaryTag ent, string normalizedName)
        {
            ent.TagName = normalizedName;
            // UpdatedAt は BaseRepository.Stamp で自動設定
        }
    }
}
