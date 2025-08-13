using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;

namespace PhrazorApp.Models
{
    public class PhraseEditModel
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
        [Display(Name = "復習回数")]
        public int ReviewCount {get; set; }
        public List<DropItemModel> SelectedDropItems { get; set; } = new();


    }


}
