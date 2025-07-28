using Microsoft.EntityFrameworkCore;
using PhrazorApp.Common;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data.Repositories
{
    public interface IGenreRepository
    {
        Task CreateGenreAsync(MGenre genre);
        Task CreateSubGenreAsync(MSubGenre subGenre);
        Task CreateGenresWithTransactionAsync(MGenre genre, List<MSubGenre> subGenres);
        Task<List<MGenre>> GetAllGenresAsync(string? userId);
        Task<MGenre?> GetGenreByIdAsync(Guid genreId, string? userId);
        Task UpdateGenreAsync(MGenre genre);
        Task DeleteGenreAsync(Guid genreId);
    }

    public class GenreRepository : IGenreRepository
    {
        private readonly IDbContextFactory<EngDbContext> _dbContextFactory;

        public GenreRepository(IDbContextFactory<EngDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        /// <summary>
        /// 単一のジャンルを追加します。
        /// </summary>
        /// <param name="genre">追加するジャンル</param>
        public async Task CreateGenreAsync(MGenre genre)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            await context.MGenres.AddAsync(genre);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// 単一のサブジャンルを追加します。
        /// </summary>
        /// <param name="subGenre">追加するサブジャンル</param>
        public async Task CreateSubGenreAsync(MSubGenre subGenre)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            await context.MSubGenres.AddAsync(subGenre);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// ジャンルとそのサブジャンルを一度に追加します。
        /// トランザクションを使用して一貫性を確保します。
        /// </summary>
        /// <param name="genre">追加するジャンル</param>
        /// <param name="subGenres">追加するサブジャンルのリスト</param>
        public async Task CreateGenresWithTransactionAsync(MGenre genre, List<MSubGenre> subGenres)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // ジャンルを追加
                await context.MGenres.AddAsync(genre);
                // サブジャンルを追加
                if (subGenres != null && subGenres.Count > 0)
                {
                    await context.MSubGenres.AddRangeAsync(subGenres);
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
        public async Task<List<MGenre>> GetAllGenresAsync(string? userId)
        {
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
        public async Task<MGenre?> GetGenreByIdAsync(Guid genreId, string? userId)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.MGenres
                .Include(x => x.MSubGenres)
                .FirstOrDefaultAsync(x => x.GenreId == genreId && x.UserId == userId);
        }

        /// <summary>
        /// ジャンルを更新します。
        /// </summary>
        /// <param name="genre">更新するジャンル</param>
        public async Task UpdateGenreAsync(MGenre genre)
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            context.MGenres.Update(genre);
            await context.SaveChangesAsync();
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