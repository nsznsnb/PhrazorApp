using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PhrazorApp.Data;
using PhrazorApp.Data.Repositories;

namespace PhrazorApp.Data.UnitOfWork
{
    /// <summary>
    /// スコープDIのUnitOfWork。
    /// - IDbContextFactoryからDbContextを遅延生成
    /// - Repositoriesも遅延生成し同一Contextを共有
    /// - Begin/Commit/RollbackでTx管理
    /// </summary>
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly IDbContextFactory<EngDbContext> _factory;
        private EngDbContext? _context;
        private IDbContextTransaction? _tx;

        // Repos cache
        private PhraseRepository? _phrases;
        private PhraseImageRepository? _phraseImages;
        private PhraseGenreRepository? _phraseGenres;
        private GenreRepository? _genres;
        private DailyUsageRepository? _dailyUsages;
        private DiaryTagRepository? _diaryTags;
        private EnglishDiaryRepository? _englishDiaries;
        private GradeRepository? _grades;
        private OperationTypeRepository? _operationTypes;
        private ProverbRepository? _proverbs;
        private ReviewLogRepository? _reviewLogs;
        private ReviewTypeRepository? _reviewTypes;
        private SubGenreRepository? _subGenres;
        private TestResultDetailRepository? _testResultDetails;
        private TestResultRepository? _testResults;

        public UnitOfWork(IDbContextFactory<EngDbContext> factory)
        {
            _factory = factory;
        }

        public EngDbContext Context => _context ?? throw new InvalidOperationException("Call BeginAsync() before accessing Context.");

        public async Task BeginAsync(CancellationToken ct = default)
        {
            if (_context != null) return; // already initialized in this scope
            _context = await _factory.CreateDbContextAsync(ct);
            _tx = await _context.Database.BeginTransactionAsync(ct);
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default) => Context.SaveChangesAsync(ct);

        public async Task CommitAsync(CancellationToken ct = default)
        {
            await Context.SaveChangesAsync(ct);
            if (_tx != null) await _tx.CommitAsync(ct);
        }

        public async Task RollbackAsync(CancellationToken ct = default)
        {
            if (_tx != null) await _tx.RollbackAsync(ct);
        }

        public async ValueTask DisposeAsync()
        {
            if (_tx != null) await _tx.DisposeAsync();
            if (_context != null) await _context.DisposeAsync();
        }

        // Lazy repos
        public PhraseRepository Phrases => _phrases ??= new PhraseRepository(Context);
        public PhraseImageRepository PhraseImages => _phraseImages ??= new PhraseImageRepository(Context);
        public PhraseGenreRepository PhraseGenres => _phraseGenres ??= new PhraseGenreRepository(Context);
        public GenreRepository Genres => _genres ??= new GenreRepository(Context);
        public DailyUsageRepository DailyUsages => _dailyUsages ??= new DailyUsageRepository(Context);
        public DiaryTagRepository DiaryTags => _diaryTags ??= new DiaryTagRepository(Context);
        public EnglishDiaryRepository EnglishDiaries => _englishDiaries ??= new EnglishDiaryRepository(Context);
        public GradeRepository Grades => _grades ??= new GradeRepository(Context);
        public OperationTypeRepository OperationTypes => _operationTypes ??= new OperationTypeRepository(Context);
        public ProverbRepository Proverbs => _proverbs ??= new ProverbRepository(Context);
        public ReviewLogRepository ReviewLogs => _reviewLogs ??= new ReviewLogRepository(Context);
        public ReviewTypeRepository ReviewTypes => _reviewTypes ??= new ReviewTypeRepository(Context);
        public SubGenreRepository SubGenres => _subGenres ??= new SubGenreRepository(Context);
        public TestResultDetailRepository TestResultDetails => _testResultDetails ??= new TestResultDetailRepository(Context);
        public TestResultRepository TestResults => _testResults ??= new TestResultRepository(Context);
    }
}
