using PhrazorApp.Data.Entities;
using PhrazorApp.Models.ViewModels;

public static class CategoryMapper
{
    public static LargeCategoryModel ToModel(this MLargeCategory entity)
    {
        return new LargeCategoryModel
        {
            Id = entity.LargeId,
            Name = entity.LargeCategoryName,
            SubCategories = entity.MSmallCategories?
                .Select(x => new SmallCategoryModel
                {
                    Id = x.SmallId,
                    Name = x.SmallCategoryName,
                    SortOrder = x.SortOrder
                })
                .OrderBy(x => x.SortOrder)
                .ToList()
        };
    }

    public static MLargeCategory ToEntity(this LargeCategoryModel model, string? userId, DateTime now)
    {
        return new MLargeCategory
        {
            LargeId = model.Id,
            LargeCategoryName = model.Name,
            CreatedAt = now,
            UpdatedAt = now,
            UserId = userId
        };
    }

    public static List<MSmallCategory>? ToSmallEntities(this LargeCategoryModel model, string? userId, DateTime now)
    {
        return model.SubCategories?.Select(x => new MSmallCategory
        {
            SmallId = x.Id,
            LargeId = model.Id,
            SmallCategoryName = x.Name,
            SortOrder = x.SortOrder,
            CreatedAt = now,
            UpdatedAt = now,
            UserId = userId
        }).ToList();
    }
}
