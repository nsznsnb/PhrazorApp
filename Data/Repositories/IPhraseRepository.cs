using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public interface IPhraseRepository
    {
        void CreatePhrase(EngDbContext context, DPhrase phrase);
        void CreatePhraseGenreRange(EngDbContext context, IEnumerable<MPhraseGenre> phraseGenres);
        void CreatePhraseImage(EngDbContext context, DPhraseImage image);
        void DeletePhrase(EngDbContext context, DPhrase phrase);
        void DeletePhraseGenreRange(EngDbContext context, IEnumerable<MPhraseGenre> phraseGenres);
        void DeletePhraseImage(EngDbContext context, DPhraseImage image);
        Task<List<DPhrase>> GetAllPhrasesAsync();
        Task<DPhrase?> GetPhraseByIdAsync(EngDbContext context, Guid? phraseId);
        Task<DPhrase?> GetPhraseByIdAsync(Guid? phraseId);
        void UpdatePhrase(EngDbContext context, DPhrase phrase);
        void UpdatePhraseImage(EngDbContext context, DPhraseImage image);
    }
}