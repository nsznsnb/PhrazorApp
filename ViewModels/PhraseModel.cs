namespace PhrazorApp.ViewModels
{
    public class PhraseModel
    {

        public string Id { get; set; }
        public string Phrase { get; set; }

        public string Meaning { get; set; }

        public string ImageUrl { get; set; }

        public string Note { get; set; }

        public List<LargeCategoryModel> LargeCategories { get; set; }


    }
}
