using FluentValidation;
using PhrazorApp.Commons;
using PhrazorApp.Models;
using System.ComponentModel.DataAnnotations;



namespace PhrazorApp.Models
{

    public class GenreModel
    {
        [Display(Name = "ジャンルId")]
        public Guid Id { get; set; }

        [Display(Name = "ジャンル名")]
        public string Name { get; set; } = string.Empty;

        public List<SubGenreModel>? SubGenres { get; set; } = new();


    }

    public class SubGenreModel
    {


        [Display(Name = "サブジャンルId")]
        public Guid Id { get; set; }

        [Display(Name = "サブジャンル名")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "ソート順")]
        public int OrderNo { get; set; }

        public bool IsDefault { get; set; } 
    }


}


