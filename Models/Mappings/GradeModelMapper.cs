using System.Linq.Expressions;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Models.Mappings
{
    public static class GradeModelMapper
    {
        public static GradeModel ToModel(this MGrade e) => new()
        {
            Id = e.GradeId,
            Name = e.GradeName ?? string.Empty,
        };

        public static void ApplyTo(this GradeModel m, MGrade e)
        {
            e.GradeName = m.Name;
        }

        public static MGrade ToEntity(this GradeModel m) => new()
        {
            GradeId = m.Id,
            GradeName = m.Name
        };

        public static readonly Expression<Func<MGrade, GradeModel>> ListProjection
            = e => new GradeModel
            {
                Id = e.GradeId,
                Name = e.GradeName ?? string.Empty,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            };

        public static IQueryable<GradeModel> SelectModel(this IQueryable<MGrade> q)
            => q.Select(ListProjection);
    }
}
