namespace PhrazorApp.Models.ViewModels
{
    public class DropItemModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public DropItemType DropTarget { get; set; } = DropItemType.UnAssigned;
        public Guid LargeCategoryId { get; set; }
        public Guid SmallCategoryId { get; set; }
    }
}
