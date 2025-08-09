using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data;
using PhrazorApp.Data.Repositories;

namespace PhrazorApp.Data.UnitOfWork
{
    /// <summary>
    /// 超シンプル UoW：
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
            public GradeRepository Grades { get; }
            public OperationTypeRepository OperationTypes { get; }
            public ProverbRepository Proverbs { get; }
            public ReviewLogRepository ReviewLogs { get; }
            public ReviewTypeRepository ReviewTypes { get; }
            public SubGenreRepository SubGenres { get; }
            public TestResultDetailRepository TestResultDetails { get; }
            public TestResultRepository TestResults { get; }

            public Repos(EngDbContext ctx)
            {
                Phrases = new PhraseRepository(ctx);
                PhraseImages = new PhraseImageRepository(ctx);
                PhraseGenres = new PhraseGenreRepository(ctx);
                Genres = new GenreRepository(ctx);
                DailyUsages = new DailyUsageRepository(ctx);
                DiaryTags = new DiaryTagRepository(ctx);
                EnglishDiaries = new EnglishDiaryRepository(ctx);
                Grades = new GradeRepository(ctx);
                OperationTypes = new OperationTypeRepository(ctx);
                Proverbs = new ProverbRepository(ctx);
                ReviewLogs = new ReviewLogRepository(ctx);
                ReviewTypes = new ReviewTypeRepository(ctx);
                SubGenres = new SubGenreRepository(ctx);
                TestResultDetails = new TestResultDetailRepository(ctx);
                TestResults = new TestResultRepository(ctx);
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
            await using var ctx = await _factory.CreateDbContextAsync();
            await using var tx = await ctx.Database.BeginTransactionAsync();
            var repos = new Repos(ctx);

            try
            {
                await work(repos);        // RepoはAdd/Update/Removeだけ。Saveはここで1回
                await ctx.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch
            {
                try { await tx.RollbackAsync(); } catch { /* ログするならここ */ }
                throw;
            }
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
