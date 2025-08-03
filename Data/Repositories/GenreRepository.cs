using Humanizer.Localisation;
using Microsoft.EntityFrameworkCore;
using PhrazorApp.Commons;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public class GenreRepository : BaseRepository<MGenre>
    {


        /// <summary>
        /// ユーザーIDに基づいて、すべてのジャンルを取得します。
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>ジャンルのリスト</returns>
        public async Task<List<MGenre>> GetAllGenresAsync(EngDbContext context, string userId)
        {
            return await context.MGenres
                .Where(x => x.UserId == userId)
                .Include(x => x.MSubGenres)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// 指定されたジャンルIDに基づいて、ジャンルを取得します。
        /// </summary>
        /// <param name="genreId">ジャンルID</param>
        /// <param name="userId">ユーザーID</param>
        /// <returns>指定されたジャンル</returns>
        public async Task<MGenre?> GetGenreByIdAsync(EngDbContext context, Guid genreId, string userId)
        {

            return await context.MGenres
                .Include(x => x.MSubGenres)
                .FirstOrDefaultAsync(x => x.GenreId == genreId && x.UserId == userId);
        }



       
    }
}