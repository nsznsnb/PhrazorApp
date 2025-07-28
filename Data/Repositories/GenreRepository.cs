using Humanizer.Localisation;
using Microsoft.EntityFrameworkCore;
using PhrazorApp.Constants;
using PhrazorApp.Data.Entities;
using PhrazorApp.Utils;

namespace PhrazorApp.Data.Repositories
{
    public class GenreRepository : IGenreRepository
    {
        private readonly IDbContextFactory<EngDbContext> _dbContextFactory;


        public GenreRepository(IDbContextFactory<EngDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }


        /// <summary>
        /// ジャンルとそのサブジャンルを一度に追加します。
        /// トランザクションを使用して一貫性を確保します。
        /// </summary>
        /// <param name="genre">追加するジャンル</param>
        /// <param name="subGenres">追加するサブジャンルのリスト</param>
        public async Task CreateGenreAsync(MGenre genre)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                genre.CreatedAt = DateTime.UtcNow;
                genre.UpdatedAt = DateTime.UtcNow;
                genre.UserId = UserUtil.GetUserId();
                // ジャンルを追加
                await context.MGenres.AddAsync(genre);
                // サブジャンルを追加
                if (genre.MSubGenres != null && genre.MSubGenres.Count > 0)
                {
                    foreach (var subGenre in genre.MSubGenres)
                    {
                        subGenre.CreatedAt = DateTime.UtcNow;
                        subGenre.UpdatedAt = DateTime.UtcNow;
                        subGenre.UserId = UserUtil.GetUserId();
                    }
                    await context.MSubGenres.AddRangeAsync(genre.MSubGenres);
                }

                // 保存してトランザクションをコミット
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(string.Format(ComMessage.MSG_E_ERROR_DETEIL, "ジャンルの作成中"), ex);
            }
        }

        /// <summary>
        /// ユーザーIDに基づいて、すべてのジャンルを取得します。
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>ジャンルのリスト</returns>
        public async Task<List<MGenre>> GetAllGenresAsync()
        {
            var userId = UserUtil.GetUserId();
            await using var context = await _dbContextFactory.CreateDbContextAsync();
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
        public async Task<MGenre?> GetGenreByIdAsync(EngDbContext context, Guid genreId)
        {

            var userId = UserUtil.GetUserId();
            return await context.MGenres
                .Include(x => x.MSubGenres)
                .FirstOrDefaultAsync(x => x.GenreId == genreId && x.UserId == userId);
        }

        /// <summary>
        /// 指定されたジャンルIDに基づいて、ジャンルを取得します。
        /// </summary>
        /// <param name="genreId">ジャンルID</param>
        /// <param name="userId">ユーザーID</param>
        /// <returns>指定されたジャンル</returns>
        public async Task<MGenre?> GetGenreByIdAsync(Guid genreId)
        {

            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await GetGenreByIdAsync(context, genreId);
        }

        /// <summary>
        /// ジャンルとそのサブジャンルを一度に追加します。
        /// トランザクションを使用して一貫性を確保します。
        /// </summary>
        /// <param name="genre">追加するジャンル</param>
        public async Task UpdateGenreAsync(MGenre genre)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            try
            {
                var saved = await context.MGenres
                    .Include(x => x.MSubGenres)
                    .FirstOrDefaultAsync(x => x.GenreId == genre.GenreId);

                if (saved == null)
                {
                    throw new Exception(string.Format(ComMessage.MSG_E_NOT_FOUND, "指定されたジャンル"));
                }

                saved.GenreId = genre.GenreId;
                saved.GenreName = genre.GenreName;
                saved.UpdatedAt = DateTime.UtcNow;
                saved.UserId = UserUtil.GetUserId();
                // ジャンルを追加
                context.MGenres.Update(saved);

                context.MSubGenres.RemoveRange(saved.MSubGenres);

                // サブジャンルを追加
                if (genre.MSubGenres != null && genre.MSubGenres.Count > 0)
                {
                    await context.MSubGenres.AddRangeAsync(genre.MSubGenres);
                }

                // 保存してトランザクションをコミット
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(ComMessage.MSG_E_ERROR_DETEIL, "ジャンルの作成中"), ex);
            }
        }

        /// <summary>
        /// ジャンルを削除します。
        /// その関連するサブジャンルも一緒に削除します。
        /// </summary>
        /// <param name="genreId">削除するジャンルID</param>
        public async Task DeleteGenreAsync(Guid genreId)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var genre = await context.MGenres
                .Include(x => x.MSubGenres)
                .FirstOrDefaultAsync(x => x.GenreId == genreId);

            if (genre != null)
            {
                // サブジャンルを削除
                context.MSubGenres.RemoveRange(genre.MSubGenres);
                // ジャンルを削除
                context.MGenres.Remove(genre);
                await context.SaveChangesAsync();
            }
            else
            {
                throw new Exception(string.Format(ComMessage.MSG_E_NOT_FOUND, "指定されたジャンル"));
            }
        }
    }
}