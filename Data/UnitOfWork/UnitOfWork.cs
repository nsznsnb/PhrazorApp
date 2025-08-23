using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Repositories;

namespace PhrazorApp.Data.UnitOfWork
{
    /// <summary>
    /// 超シンプル UoW（CancellationToken 不使用）：
    /// - ReadAsync: 参照（Txなし、NoTracking、Saveなし）
    /// - ExecuteInTransactionAsync: 更新（Txあり、最後に1回だけ Save+Commit）
    /// - Repos はラムダ内専用。Contextを外に出さないので誤用しづらい。
    /// </summary>
    public sealed class UnitOfWork : IAsyncDisposable
    {
        private readonly IDbContextFactory<EngDbContext> _factory;
        public UnitOfWork(IDbContextFactory<EngDbContext> factory) => _factory = factory;

        // リポジトリ束（必要に応じて追加）
        public sealed class Repos
        {
            public PhraseRepository Phrases { get; }
            public PhraseImageRepository PhraseImages { get; }
            public PhraseGenreRepository PhraseGenres { get; }
            public GenreRepository Genres { get; }
            public DailyUsageRepository DailyUsages { get; }
            public DiaryTagRepository DiaryTags { get; }
            public EnglishDiaryRepository EnglishDiaries { get; }

            public EnglishDiaryTagRepository EnglishDiaryTags { get; }
            public GradeRepository Grades { get; }
            public OperationTypeRepository OperationTypes { get; }
            public ProverbRepository Proverbs { get; }
            public ReviewLogRepository ReviewLogs { get; }
            public ReviewTypeRepository ReviewTypes { get; }
            public SubGenreRepository SubGenres { get; }
            public TestResultDetailRepository TestResultDetails { get; }
            public TestResultRepository TestResults { get; }
            public PhraseBookRepository PhraseBooks { get; }
            public PhraseBookItemRepository PhraseBookItems { get; }

            public Repos(EngDbContext ctx)
            {
                Phrases = new PhraseRepository(ctx);
                PhraseImages = new PhraseImageRepository(ctx);
                PhraseGenres = new PhraseGenreRepository(ctx);
                Genres = new GenreRepository(ctx);
                DailyUsages = new DailyUsageRepository(ctx);
                DiaryTags = new DiaryTagRepository(ctx);
                EnglishDiaries = new EnglishDiaryRepository(ctx);
                EnglishDiaryTags = new EnglishDiaryTagRepository(ctx);
                Grades = new GradeRepository(ctx);
                OperationTypes = new OperationTypeRepository(ctx);
                Proverbs = new ProverbRepository(ctx);
                ReviewLogs = new ReviewLogRepository(ctx);
                ReviewTypes = new ReviewTypeRepository(ctx);
                SubGenres = new SubGenreRepository(ctx);
                TestResultDetails = new TestResultDetailRepository(ctx);
                TestResults = new TestResultRepository(ctx);
                PhraseBooks = new PhraseBookRepository(ctx);
                PhraseBookItems = new PhraseBookItemRepository(ctx);
            }
        }

        // 読み取り専用（Txなし・NoTracking・Saveなし）
        public async Task<T> ReadAsync<T>(Func<Repos, Task<T>> work)
        {
            await using var ctx = await _factory.CreateDbContextAsync();
            ctx.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            var repos = new Repos(ctx);
            return await work(repos);
        }

        // 書き込み（Txあり・最後に1回だけ Save + Commit）
        public async Task ExecuteInTransactionAsync(Func<Repos, Task> work)
        {
            // 1) 毎回フレッシュな DbContext（失敗後に再利用しない）
            await using var ctx = await _factory.CreateDbContextAsync();

            // 2) 大量処理向け最適化：自動変更検出をOFF（常時）
            var originalAutoDetect = ctx.ChangeTracker.AutoDetectChangesEnabled;
            ctx.ChangeTracker.AutoDetectChangesEnabled = false;

            // 3) 一時障害向けの実行戦略（内部で必要に応じて再試行）
            var strategy = ctx.Database.CreateExecutionStrategy();

            try
            {
                await strategy.ExecuteAsync(async () =>
                {
                    // 4) 明示トランザクション
                    await using var tx = await ctx.Database.BeginTransactionAsync();
                    try
                    {
                        var repos = new Repos(ctx);

                        // ※必ず Add/Remove/Update or Entry(...).State/IsModified を使う
                        await work(repos);

                        // 5) SaveChanges は1回だけ
                        await ctx.SaveChangesAsync();

                        await tx.CommitAsync();
                    }
                    catch
                    {
                        try { await tx.RollbackAsync(); } catch(Exception ex) { Console.WriteLine(ex); }
                        throw;
                    }
                });
            }
            finally
            {
                // 6) フラグを元に戻す
                ctx.ChangeTracker.AutoDetectChangesEnabled = originalAutoDetect;
            }
        }

        /// <summary>
        /// 書き込みあり(戻り値あり)
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="work"></param>
        /// <returns></returns>
        public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Repos, Task<TResult>> work)
        {
            await using var ctx = await _factory.CreateDbContextAsync();

            var originalAutoDetect = ctx.ChangeTracker.AutoDetectChangesEnabled;
            ctx.ChangeTracker.AutoDetectChangesEnabled = false;

            var strategy = ctx.Database.CreateExecutionStrategy();
            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await ctx.Database.BeginTransactionAsync();
                    try
                    {
                        var repos = new Repos(ctx);
                        var result = await work(repos);
                        await ctx.SaveChangesAsync();
                        await tx.CommitAsync();
                        return result;
                    }
                    catch
                    {
                        try { await tx.RollbackAsync(); } catch { }
                        throw;
                    }
                });
            }
            finally
            {
                ctx.ChangeTracker.AutoDetectChangesEnabled = originalAutoDetect;
            }
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
