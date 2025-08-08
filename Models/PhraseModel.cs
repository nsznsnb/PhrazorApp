using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;

namespace PhrazorApp.Models
{
    public class PhraseModel
    {
        [Ignore]
        public Guid Id { get; set; }

        [Index(0)]
        [Display(Name = "フレーズ")]
        public string Phrase { get; set; } = string.Empty;

        [Index(1)]
        [Display(Name = "意味")]
        public string Meaning { get; set; } = string.Empty;
        [Ignore]
        [Display(Name = "画像")]
        public string ImageUrl { get; set; } = string.Empty;
        [Ignore]
        [Display(Name = "Note")]
        public string Note { get; set; } = string.Empty;
        [Ignore]
        [Display(Name = "復習回数")]
        public int ReviewCount {get; set; }
        [Ignore]
        public List<DropItemModel> SelectedDropItems { get; set; } = new();


    }


}
