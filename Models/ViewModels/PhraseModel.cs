using System.ComponentModel.DataAnnotations;

namespace PhrazorApp.Models.ViewModels
{
    public class PhraseModel
    {

        public Guid Id { get; set; }

        [Display(Name = "フレーズ")]
        public string Phrase { get; set; } = string.Empty;

        [Display(Name = "意味")]
        public string Meaning { get; set; } = string.Empty;

        [Display(Name = "画像")]
        public string ImageUrl { get; set; } = string.Empty;

        [Display(Name = "Note")]
        public string Note { get; set; } = string.Empty;

        // Drop用
        public List<DropItemModel> AllCategoryItems { get; set; } = new();
        public List<DropItemModel> SelectedItems { get; set; } = new();


    }


}
