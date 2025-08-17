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
            OrderNo = e.OrderNo,
        };

        public static void ApplyTo(this GradeModel m, MGrade e)
        {
            e.GradeName = m.Name;
            e.OrderNo = m.OrderNo;
        }

        // ★ 追加：Entity 生成（Create 用）
        public static MGrade ToEntity(this GradeModel m)
        {
            return new MGrade
            {
                GradeId = m.Id == Guid.Empty ? Guid.NewGuid() : m.Id,
                GradeName = m.Name,
                OrderNo = m.OrderNo,
                // ※ UserId / CreatedAt / UpdatedAt は既存のリポジトリ／UoWのスタンプに任せます
            };
        }

        public static readonly Expression<Func<MGrade, GradeModel>> ListProjection
            = e => new GradeModel
            {
                Id = e.GradeId,
                Name = e.GradeName ?? string.Empty,
                OrderNo = e.OrderNo,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            };

        public static IQueryable<GradeModel> SelectModel(this IQueryable<MGrade> q)
            => q.Select(ListProjection);
    }
}
