using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public interface IGenreRepository
    {
        Task CreateGenreAsync(MGenre genre);
        Task DeleteGenreAsync(Guid genreId);
        Task<List<MGenre>> GetAllGenresAsync();
        Task<MGenre?> GetGenreByIdAsync(EngDbContext context, Guid genreId);
        Task<MGenre?> GetGenreByIdAsync(Guid genreId);
        Task UpdateGenreAsync(MGenre genre);
    }
}