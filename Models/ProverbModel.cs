using System.ComponentModel.DataAnnotations;

namespace PhrazorApp.Models
{
    public class ProverbModel
    {
        public Guid Id { get; set; }

        [Display(Name = "格言"), Required, StringLength(200)]
        public string Text { get; set; } = string.Empty;

        [Display(Name = "意味"), StringLength(200)]
        public string? Meaning { get; set; }

        [Display(Name = "著者"), StringLength(100)]
        public string? Author { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
