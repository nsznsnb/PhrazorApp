using PhrazorApp.Data;

namespace PhrazorApp.Data.UnitOfWork
{
    /// <summary>
    /// スコープDI版のUnitOfWork。BeginAsyncでContextを遅延生成し、同一スコープで使い回す。
    /// </summary>
    public interface IUnitOfWork : IAsyncDisposable
    {
        EngDbContext Context { get; }   // BeginAsync()前はInvalidOperationException

        Task BeginAsync(CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task CommitAsync(CancellationToken ct = default);
        Task RollbackAsync(CancellationToken ct = default);

        // Repositories（Context初期化後に遅延生成）
        PhrazorApp.Data.Repositories.PhraseRepository Phrases { get; }
        PhrazorApp.Data.Repositories.PhraseImageRepository PhraseImages { get; }
        PhrazorApp.Data.Repositories.PhraseGenreRepository PhraseGenres { get; }
        PhrazorApp.Data.Repositories.GenreRepository Genres { get; }
        PhrazorApp.Data.Repositories.DailyUsageRepository DailyUsages { get; }
        PhrazorApp.Data.Repositories.DiaryTagRepository DiaryTags { get; }
        PhrazorApp.Data.Repositories.EnglishDiaryRepository EnglishDiaries { get; }
        PhrazorApp.Data.Repositories.GradeRepository Grades { get; }
        PhrazorApp.Data.Repositories.OperationTypeRepository OperationTypes { get; }
        PhrazorApp.Data.Repositories.ProverbRepository Proverbs { get; }
        PhrazorApp.Data.Repositories.ReviewLogRepository ReviewLogs { get; }
        PhrazorApp.Data.Repositories.ReviewTypeRepository ReviewTypes { get; }
        PhrazorApp.Data.Repositories.SubGenreRepository SubGenres { get; }
        PhrazorApp.Data.Repositories.TestResultDetailRepository TestResultDetails { get; }
        PhrazorApp.Data.Repositories.TestResultRepository TestResults { get; }
    }
}
