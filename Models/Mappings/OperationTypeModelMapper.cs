using System.Linq.Expressions;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Models.Mappings
{
    public static class OperationTypeModelMapper
    {
        public static OperationTypeModel ToModel(this MOperationType e) => new()
        {
            Id = e.OperationTypeId,
            Code = e.OperationTypeCode ?? string.Empty,
            Name = e.OperationTypeName ?? string.Empty,
            Limit = e.OperationTypeLimit,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };

        public static void ApplyTo(this OperationTypeModel m, MOperationType e)
        {
            e.OperationTypeCode = m.Code;
            e.OperationTypeName = m.Name;
            e.OperationTypeLimit = m.Limit;
        }

        public static MOperationType ToEntity(this OperationTypeModel m) => new()
        {
            OperationTypeId = m.Id,
            OperationTypeCode = m.Code,
            OperationTypeName = m.Name,
            OperationTypeLimit = m.Limit
        };

        public static readonly Expression<Func<MOperationType, OperationTypeModel>> ListProjection
            = e => new OperationTypeModel
            {
                Id = e.OperationTypeId,
                Code = e.OperationTypeCode ?? string.Empty,
                Name = e.OperationTypeName ?? string.Empty,
                Limit = e.OperationTypeLimit,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            };

        public static IQueryable<OperationTypeModel> SelectModel(this IQueryable<MOperationType> q)
            => q.Select(ListProjection);
    }
}
