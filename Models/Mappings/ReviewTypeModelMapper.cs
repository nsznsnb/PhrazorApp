using System.Linq.Expressions;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Models.Mappings
{
    public static class ReviewTypeModelMapper
    {
        public static ReviewTypeModel ToModel(this MReviewType e) => new()
        {
            Id = e.ReviewTypeId,
            Name = e.ReviewTypeName ?? string.Empty,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };

        public static void ApplyTo(this ReviewTypeModel m, MReviewType e)
        {
            e.ReviewTypeName = m.Name;
        }

        public static MReviewType ToEntity(this ReviewTypeModel m) => new()
        {
            ReviewTypeId = m.Id,
            ReviewTypeName = m.Name
        };

        public static readonly Expression<Func<MReviewType, ReviewTypeModel>> ListProjection
            = e => new ReviewTypeModel
            {
                Id = e.ReviewTypeId,
                Name = e.ReviewTypeName ?? string.Empty,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            };

        public static IQueryable<ReviewTypeModel> SelectModel(this IQueryable<MReviewType> q)
            => q.Select(ListProjection);
    }
}
