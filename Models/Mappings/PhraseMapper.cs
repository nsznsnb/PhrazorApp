using PhrazorApp.Data.Entities;
using PhrazorApp.Models.ViewModels;

namespace PhrazorApp.Models.Mappings;

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
    /// DropItemModel → MPhraseCategory リストへ変換
    /// </summary>
    public static List<MPhraseCategory> ToPhraseCategoryEntities(
        this List<DropItemModel> dropItems,
        Guid phraseId,
        DateTime now)
    {
        return dropItems
            .Where(item => item.DropTarget == DropItemType.Target)
            .Select(item => new MPhraseCategory
            {
                PhraseId = phraseId,
                LargeId = item.LargeCategoryId,
                SmallId = item.SmallCategoryId,
                CreatedAt = now,
                UpdatedAt = now
            })
            .ToList();
    }

    public static List<DropItemModel> ToDropItemModelList(
      this List<MLargeCategory> allCategories,
      List<MPhraseCategory> selectedCategories)
    {
        var result = new List<DropItemModel>();

        foreach (var large in allCategories)
        {
            foreach (var small in large.MSmallCategories)
            {
                var isSelected = selectedCategories.Any(x => x.SmallId == small.SmallId);
                result.Add(new DropItemModel
                {
                    Id = small.SmallId,
                    Name = small.SmallCategoryName,
                    Group = large.LargeCategoryName,
                    DropTarget = isSelected ? DropItemType.Target : DropItemType.UnAssigned,
                    LargeCategoryId = large.LargeId,
                    SmallCategoryId = small.SmallId
                });
            }
        }

        return result;
    }
}
