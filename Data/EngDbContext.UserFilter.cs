// EngDbContext.UserFilter.cs（partial）
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data.Entities;
using System.Security.Claims;

namespace PhrazorApp.Data   // ← ★ これが必須
{
    public partial class EngDbContext : DbContext
    {
        private readonly string? _uid;

        // HttpContextAccessor は Singleton なので Root でも解決可
        [ActivatorUtilitiesConstructor]
        public EngDbContext(DbContextOptions<EngDbContext> options, IHttpContextAccessor accessor)
            : base(options)
        {
            _uid = accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }


        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            var uid = _uid;
            if (string.IsNullOrEmpty(uid)) return;

            // ここに HasQueryFilter(...) をこれまで通り設定
            modelBuilder.Entity<DPhrase>().HasQueryFilter(e => e.UserId == uid);
            modelBuilder.Entity<DEnglishDiary>().HasQueryFilter(e => e.UserId == uid);
            modelBuilder.Entity<DDailyUsage>().HasQueryFilter(e => e.UserId == uid);
            modelBuilder.Entity<DTestResult>().HasQueryFilter(e => e.UserId == uid);
            modelBuilder.Entity<MGenre>().HasQueryFilter(e => e.UserId == uid);
            modelBuilder.Entity<MSubGenre>().HasQueryFilter(e => e.UserId == uid);
            modelBuilder.Entity<MDiaryTag>().HasQueryFilter(e => e.UserId == uid);
            modelBuilder.Entity<MPhraseBook>().HasQueryFilter(e => e.UserId == uid);

            modelBuilder.Entity<DPhraseImage>().HasQueryFilter(e => e.Phrase.UserId == uid);
            modelBuilder.Entity<MPhraseGenre>().HasQueryFilter(e => e.Phrase.UserId == uid);
            modelBuilder.Entity<DTestResultDetail>().HasQueryFilter(e => e.Test.UserId == uid);
            modelBuilder.Entity<DReviewLog>().HasQueryFilter(e => e.Phrase.UserId == uid);
            modelBuilder.Entity<DEnglishDiaryTag>().HasQueryFilter(e => e.Diary.UserId == uid);
            modelBuilder.Entity<MPhraseBookItem>().HasQueryFilter(e => e.PhraseBook.UserId == uid);
        }
    }

}