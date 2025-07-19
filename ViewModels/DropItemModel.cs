namespace PhrazorApp.ViewModels
{
    public class DropItemModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty; // 例: "Large", "Small" ではなく "Primary", "Secondary" など
        public string DropTarget { get; set; } = string.Empty;
    }
}
