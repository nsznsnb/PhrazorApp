// EngDbContext.UserFilter.cs（partial）
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data.Entities;
using System.Security.Claims;

namespace PhrazorApp.Data
{
    public partial class EngDbContext : DbContext
    {
        private readonly string? _uid;

        [ActivatorUtilitiesConstructor]
        public EngDbContext(DbContextOptions<EngDbContext> options, IHttpContextAccessor accessor)
            : base(options)
        {
            _uid = accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // ※ デザイン時コンストラクタは別 partial にあるので省略

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            // ★ 重要：
            // - ローカル変数(uid)にコピーして HasQueryFilter(e => e.UserId == uid) と書かない
            //   （モデルキャッシュ時に値が固定化される恐れがあるため）
            // - 常に _uid を直接参照する式にする
            // - _uid != null をガードにつけると未ログイン時は常に false になり 0件返す

            // UserId を直接持つテーブル
            modelBuilder.Entity<DPhrase>()
                .HasQueryFilter(e => _uid != null && e.UserId == _uid);
            modelBuilder.Entity<DEnglishDiary>()
                .HasQueryFilter(e => _uid != null && e.UserId == _uid);
            modelBuilder.Entity<DDailyUsage>()
                .HasQueryFilter(e => _uid != null && e.UserId == _uid);
            modelBuilder.Entity<DTestResult>()
                .HasQueryFilter(e => _uid != null && e.UserId == _uid);
            modelBuilder.Entity<MGenre>()
                .HasQueryFilter(e => _uid != null && e.UserId == _uid);
            modelBuilder.Entity<MSubGenre>()
                .HasQueryFilter(e => _uid != null && e.UserId == _uid);
            modelBuilder.Entity<MDiaryTag>()
                .HasQueryFilter(e => _uid != null && e.UserId == _uid);
            modelBuilder.Entity<MPhraseBook>()
                .HasQueryFilter(e => _uid != null && e.UserId == _uid);

            // UserId を持たないので親ナビゲーション経由で制限するテーブル
            modelBuilder.Entity<DPhraseImage>()
                .HasQueryFilter(e => _uid != null && e.Phrase.UserId == _uid);
            modelBuilder.Entity<MPhraseGenre>()
                .HasQueryFilter(e => _uid != null && e.Phrase.UserId == _uid);
            modelBuilder.Entity<DTestResultDetail>()
                .HasQueryFilter(e => _uid != null && e.Test.UserId == _uid);
            modelBuilder.Entity<DReviewLog>()
                .HasQueryFilter(e => _uid != null && e.Phrase.UserId == _uid);
            modelBuilder.Entity<DEnglishDiaryTag>()
                .HasQueryFilter(e => _uid != null && e.Diary.UserId == _uid);
            modelBuilder.Entity<MPhraseBookItem>()
                .HasQueryFilter(e => _uid != null && e.PhraseBook.UserId == _uid);
        }
    }
}
