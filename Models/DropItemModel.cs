namespace PhrazorApp.Models
{
    public class DropItemModel
    {

        public Guid Key1 { get; set; }

        public Guid? Key2 { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
        public DropItemType DropTarget { get; set; } = DropItemType.UnAssigned;
    }
}
