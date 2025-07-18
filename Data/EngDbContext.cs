using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PhrazorApp.Models;

namespace PhrazorApp.Data;

public partial class EngDbContext : DbContext
{
    public EngDbContext(DbContextOptions<EngDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DEnglishDiary> DEnglishDiarys { get; set; }

    public virtual DbSet<DEnglishDiaryTag> DEnglishDiaryTags { get; set; }

    public virtual DbSet<DPhraseBookItem> DPhraseBookItems { get; set; }

    public virtual DbSet<DPhraseImage> DPhraseImages { get; set; }

    public virtual DbSet<DReviewLog> DReviewLogs { get; set; }

    public virtual DbSet<DReviewSchedule> DReviewSchedules { get; set; }

    public virtual DbSet<DTestResult> DTestResults { get; set; }

    public virtual DbSet<DTestResultDetail> DTestResultDetails { get; set; }

    public virtual DbSet<DUserPhrase> DUserPhrases { get; set; }

    public virtual DbSet<MDiaryTag> MDiaryTags { get; set; }

    public virtual DbSet<MGrade> MGrades { get; set; }

    public virtual DbSet<MLargeCategory> MLargeCategories { get; set; }

    public virtual DbSet<MPhraseBook> MPhraseBooks { get; set; }

    public virtual DbSet<MPhraseCategory> MPhraseCategories { get; set; }

    public virtual DbSet<MProverb> MProverbs { get; set; }

    public virtual DbSet<MReviewType> MReviewTypes { get; set; }

    public virtual DbSet<MSmallCategory> MSmallCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DEnglishDiary>(entity =>
        {
            entity.HasKey(e => e.DiaryId).HasName("D_ENGLISH_DIARYS_PKC");

            entity.ToTable("D_ENGLISH_DIARYS", tb => tb.HasComment("英語日記"));

            entity.HasIndex(e => e.DiaryId, "D_ENGLISH_DIARYS_PKI").IsUnique();

            entity.Property(e => e.DiaryId)
                .ValueGeneratedNever()
                .HasComment("英語日記ID")
                .HasColumnName("diary_id");
            entity.Property(e => e.Content)
                .HasComment("内容")
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("作成日時")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasComment("タイトル")
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasMaxLength(450)
                .HasComment("ユーザーId")
                .HasColumnName("user_id");
        });

        modelBuilder.Entity<DEnglishDiaryTag>(entity =>
        {
            entity.HasKey(e => new { e.DiaryId, e.DiaryTagId }).HasName("D_ENGLISH_DIARY_TAGS_PKC");

            entity.ToTable("D_ENGLISH_DIARY_TAGS", tb => tb.HasComment("英語日記タグ"));

            entity.HasIndex(e => new { e.DiaryId, e.DiaryTagId }, "D_ENGLISH_DIARY_TAGS_PKI").IsUnique();

            entity.Property(e => e.DiaryId)
                .HasComment("英語日記ID")
                .HasColumnName("diary_id");
            entity.Property(e => e.DiaryTagId)
                .HasComment("日記タグID")
                .HasColumnName("diary_tag_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("作成日時")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Diary).WithMany(p => p.DEnglishDiaryTags)
                .HasForeignKey(d => d.DiaryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("D_ENGLISH_DIARY_TAGS_FK2");

            entity.HasOne(d => d.DiaryTag).WithMany(p => p.DEnglishDiaryTags)
                .HasForeignKey(d => d.DiaryTagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("D_ENGLISH_DIARY_TAGS_FK1");
        });

        modelBuilder.Entity<DPhraseBookItem>(entity =>
        {
            entity.HasKey(e => new { e.PhraseBookId, e.PhraseId }).HasName("D_PHRASE_BOOK_ITEMS_PKC");

            entity.ToTable("D_PHRASE_BOOK_ITEMS", tb => tb.HasComment("フレーズ帳内フレーズ"));

            entity.HasIndex(e => new { e.PhraseBookId, e.PhraseId }, "D_PHRASE_BOOK_ITEMS_PKI").IsUnique();

            entity.Property(e => e.PhraseBookId)
                .HasComment("フレーズ帳Id")
                .HasColumnName("phrase_book_id");
            entity.Property(e => e.PhraseId)
                .HasComment("フレーズId")
                .HasColumnName("phrase_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("作成日時")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.PhraseBook).WithMany(p => p.DPhraseBookItems)
                .HasForeignKey(d => d.PhraseBookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("D_PHRASE_BOOK_ITEMS_FK1");

            entity.HasOne(d => d.Phrase).WithMany(p => p.DPhraseBookItems)
                .HasForeignKey(d => d.PhraseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("D_PHRASE_BOOK_ITEMS_FK2");
        });

        modelBuilder.Entity<DPhraseImage>(entity =>
        {
            entity.HasKey(e => e.PhraseImageId).HasName("D_PHRASE_IMAGES_PKC");

            entity.ToTable("D_PHRASE_IMAGES", tb => tb.HasComment("フレーズ画像"));

            entity.HasIndex(e => e.PhraseImageId, "D_PHRASE_IMAGES_PKI").IsUnique();

            entity.Property(e => e.PhraseImageId)
                .ValueGeneratedNever()
                .HasComment("フレーズ画像ID")
                .HasColumnName("phrase_image_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("作成日時")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.PhraseId)
                .HasComment("フレーズID")
                .HasColumnName("phrase_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UploadAt)
                .HasComment("アップロード日時")
                .HasColumnType("datetime")
                .HasColumnName("upload_at");
            entity.Property(e => e.Url)
                .HasMaxLength(500)
                .HasComment("URL")
                .HasColumnName("url");

            entity.HasOne(d => d.Phrase).WithMany(p => p.DPhraseImages)
                .HasForeignKey(d => d.PhraseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("D_PHRASE_IMAGES_FK1");
        });

        modelBuilder.Entity<DReviewLog>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("D_REVIEW_LOGS_PKC");

            entity.ToTable("D_REVIEW_LOGS", tb => tb.HasComment("復習履歴"));

            entity.HasIndex(e => e.ReviewId, "D_REVIEW_LOGS_PKI").IsUnique();

            entity.Property(e => e.ReviewId)
                .ValueGeneratedNever()
                .HasComment("復習履歴ID")
                .HasColumnName("review_id ");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("作成日時")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.PhraseId)
                .HasComment("フレーズID")
                .HasColumnName("phrase_id");
            entity.Property(e => e.ReviewDate)
                .HasDefaultValueSql("(getdate())")
                .HasComment("復習日")
                .HasColumnType("datetime")
                .HasColumnName("review_date");
            entity.Property(e => e.ReviewTypeId)
                .HasComment("復習種別ID")
                .HasColumnName("review_type_id");
            entity.Property(e => e.TestId)
                .HasComment("テスト結果ID")
                .HasColumnName("test_id");
            entity.Property(e => e.TestResultDetailNo)
                .HasComment("テスト結果明細連番")
                .HasColumnName("test_result_detail_no");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Phrase).WithMany(p => p.DReviewLogs)
                .HasForeignKey(d => d.PhraseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("D_REVIEW_LOGS_FK1");

            entity.HasOne(d => d.ReviewType).WithMany(p => p.DReviewLogs)
                .HasForeignKey(d => d.ReviewTypeId)
                .HasConstraintName("D_REVIEW_LOGS_FK3");

            entity.HasOne(d => d.DTestResultDetail).WithMany(p => p.DReviewLogs)
                .HasForeignKey(d => new { d.TestId, d.TestResultDetailNo })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("D_REVIEW_LOGS_FK2");
        });

        modelBuilder.Entity<DReviewSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("D_REVIEW_SCHEDULES_PKC");

            entity.ToTable("D_REVIEW_SCHEDULES", tb => tb.HasComment("復習スケジュール"));

            entity.HasIndex(e => e.ScheduleId, "D_REVIEW_SCHEDULES_PKI").IsUnique();

            entity.Property(e => e.ScheduleId)
                .ValueGeneratedNever()
                .HasComment("スケジュールID")
                .HasColumnName("schedule_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("作成日時")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.NextReviewDate)
                .HasComment("次回復習日")
                .HasColumnType("datetime")
                .HasColumnName("next_review_date");
            entity.Property(e => e.PhraseId)
                .HasComment("フレーズID")
                .HasColumnName("phrase_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Phrase).WithMany(p => p.DReviewSchedules)
                .HasForeignKey(d => d.PhraseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("D_REVIEW_SCHEDULES_FK1");
        });

        modelBuilder.Entity<DTestResult>(entity =>
        {
            entity.HasKey(e => e.TestId).HasName("D_TEST_RESULTS_PKC");

            entity.ToTable("D_TEST_RESULTS", tb => tb.HasComment("テスト結果"));

            entity.HasIndex(e => e.TestId, "D_TEST_RESULTS_PKI").IsUnique();

            entity.Property(e => e.TestId)
                .ValueGeneratedNever()
                .HasComment("テスト結果ID")
                .HasColumnName("test_id");
            entity.Property(e => e.CompleteFlg)
                .HasComment("完了フラグ")
                .HasColumnName("complete_flg");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("作成日時")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.GradeId)
                .HasComment("成績ID")
                .HasColumnName("grade_id");
            entity.Property(e => e.TestDatetime)
                .HasComment("テスト日時")
                .HasColumnType("datetime")
                .HasColumnName("test_datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Grade).WithMany(p => p.DTestResults)
                .HasForeignKey(d => d.GradeId)
                .HasConstraintName("D_TEST_RESULTS_FK1");
        });

        modelBuilder.Entity<DTestResultDetail>(entity =>
        {
            entity.HasKey(e => new { e.TestId, e.TestResultDetailNo }).HasName("D_TEST_RESULT_DETAILS_PKC");

            entity.ToTable("D_TEST_RESULT_DETAILS", tb => tb.HasComment("テスト結果明細"));

            entity.HasIndex(e => new { e.TestId, e.TestResultDetailNo }, "D_TEST_RESULT_DETAILS_PKI").IsUnique();

            entity.Property(e => e.TestId)
                .HasComment("テスト結果ID")
                .HasColumnName("test_id");
            entity.Property(e => e.TestResultDetailNo)
                .HasComment("テスト結果明細連番")
                .HasColumnName("test_result_detail_no");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("作成日時")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsCorrect)
                .HasDefaultValue(false)
                .HasComment("正解フラグ")
                .HasColumnName("is_correct");
            entity.Property(e => e.PhraseId)
                .HasComment("フレーズID")
                .HasColumnName("phrase_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Phrase).WithMany(p => p.DTestResultDetails)
                .HasForeignKey(d => d.PhraseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("D_TEST_RESULT_DETAILS_FK1");

            entity.HasOne(d => d.Test).WithMany(p => p.DTestResultDetails)
                .HasForeignKey(d => d.TestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("D_TEST_RESULT_DETAILS_FK2");
        });

        modelBuilder.Entity<DUserPhrase>(entity =>
        {
            entity.HasKey(e => e.PhraseId).HasName("D_USER_PHRASES_PKC");

            entity.ToTable("D_USER_PHRASES", tb => tb.HasComment("ユーザーフレーズ"));

            entity.HasIndex(e => e.PhraseId, "D_USER_PHRASES_PKI").IsUnique();

            entity.Property(e => e.PhraseId)
                .ValueGeneratedNever()
                .HasComment("フレーズID")
                .HasColumnName("phrase_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("作成日時")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Meaning)
                .HasMaxLength(200)
                .HasComment("意味")
                .HasColumnName("meaning");
            entity.Property(e => e.Note)
                .HasMaxLength(200)
                .HasComment("備考")
                .HasColumnName("note");
            entity.Property(e => e.Phrase)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasComment("フレーズ")
                .HasColumnName("phrase");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasMaxLength(450)
                .HasComment("ユーザーID")
                .HasColumnName("user_id");
        });

        modelBuilder.Entity<MDiaryTag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("M_DIARY_TAGS_PKC");

            entity.ToTable("M_DIARY_TAGS", tb => tb.HasComment("日記タグ"));

            entity.HasIndex(e => e.TagId, "M_DIARY_TAGS_PKI").IsUnique();

            entity.Property(e => e.TagId)
                .ValueGeneratedNever()
                .HasComment("日記タグID")
                .HasColumnName("tag_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("作成日時")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.TagName)
                .HasMaxLength(50)
                .HasComment("タグ名")
                .HasColumnName("tag_name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<MGrade>(entity =>
        {
            entity.HasKey(e => e.GradeId).HasName("M_GRADES_PKC");

            entity.ToTable("M_GRADES", tb => tb.HasComment("成績マスタ"));

            entity.HasIndex(e => e.GradeName, "M_GRADES_IX1").IsUnique();

            entity.HasIndex(e => e.GradeId, "M_GRADES_PKI").IsUnique();

            entity.Property(e => e.GradeId)
                .ValueGeneratedNever()
                .HasComment("成績ID")
                .HasColumnName("grade_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("作成日時")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.GradeName)
                .HasMaxLength(20)
                .HasComment("成績名")
                .HasColumnName("grade_name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<MLargeCategory>(entity =>
        {
            entity.HasKey(e => e.LargeId).HasName("M_LARGE_CATEGORIES_PKC");

            entity.ToTable("M_LARGE_CATEGORIES", tb => tb.HasComment("大分類マスタ"));

            entity.HasIndex(e => e.LargeCategoryName, "M_LARGE_CATEGORIES_IX1").IsUnique();

            entity.HasIndex(e => e.LargeId, "M_LARGE_CATEGORIES_PKI").IsUnique();

            entity.Property(e => e.LargeId)
                .ValueGeneratedNever()
                .HasComment("大分類ID")
                .HasColumnName("large_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("作成日時")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.LargeCategoryName)
                .HasMaxLength(30)
                .HasComment("大分類名")
                .HasColumnName("large_category_name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasMaxLength(450)
                .HasComment("ユーザーId")
                .HasColumnName("user_id");
        });

        modelBuilder.Entity<MPhraseBook>(entity =>
        {
            entity.HasKey(e => e.PhraseBookId).HasName("M_PHRASE_BOOKS_PKC");

            entity.ToTable("M_PHRASE_BOOKS", tb => tb.HasComment("フレーズ帳マスタ"));

            entity.HasIndex(e => e.PhraseBookId, "M_PHRASE_BOOKS_PKI").IsUnique();

            entity.Property(e => e.PhraseBookId)
                .ValueGeneratedNever()
                .HasComment("フレーズ帳Id")
                .HasColumnName("phrase_book_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("作成日時")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.PhraseBookName)
                .HasMaxLength(100)
                .HasComment("フレーズ帳名")
                .HasColumnName("phrase_book_name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasMaxLength(450)
                .HasComment("ユーザーId")
                .HasColumnName("user_id");
        });

        modelBuilder.Entity<MPhraseCategory>(entity =>
        {
            entity.HasKey(e => new { e.PhraseId, e.SmallId, e.LargeId }).HasName("M_PHRASE_CATEGORIES_PKC");

            entity.ToTable("M_PHRASE_CATEGORIES", tb => tb.HasComment("フレーズ分類"));

            entity.HasIndex(e => new { e.PhraseId, e.SmallId, e.LargeId }, "M_PHRASE_CATEGORIES_PKI").IsUnique();

            entity.Property(e => e.PhraseId)
                .HasComment("フレーズID")
                .HasColumnName("phrase_id");
            entity.Property(e => e.SmallId)
                .HasComment("小分類ID")
                .HasColumnName("small_id");
            entity.Property(e => e.LargeId)
                .HasComment("大分類ID")
                .HasColumnName("large_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("作成日時")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Phrase).WithMany(p => p.MPhraseCategories)
                .HasForeignKey(d => d.PhraseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("M_PHRASE_CATEGORIES_FK1");

            entity.HasOne(d => d.MSmallCategory).WithMany(p => p.MPhraseCategories)
                .HasForeignKey(d => new { d.LargeId, d.SmallId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("M_PHRASE_CATEGORIES_FK2");
        });

        modelBuilder.Entity<MProverb>(entity =>
        {
            entity.HasKey(e => e.ProverbId).HasName("M_PROVERBS_PKC");

            entity.ToTable("M_PROVERBS", tb => tb.HasComment("格言マスタ"));

            entity.HasIndex(e => e.ProverbId, "M_PROVERBS_PKI").IsUnique();

            entity.Property(e => e.ProverbId)
                .ValueGeneratedNever()
                .HasComment("格言Id")
                .HasColumnName("proverb_id");
            entity.Property(e => e.Author)
                .HasMaxLength(100)
                .HasComment("著者")
                .HasColumnName("author");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("作成日時")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Meaning)
                .HasMaxLength(200)
                .HasComment("意味")
                .HasColumnName("meaning");
            entity.Property(e => e.ProverbText)
                .HasMaxLength(200)
                .HasComment("格言")
                .HasColumnName("proverb_text");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<MReviewType>(entity =>
        {
            entity.HasKey(e => e.ReviewTypeId).HasName("M_REVIEW_TYPES_PKC");

            entity.ToTable("M_REVIEW_TYPES", tb => tb.HasComment("復習種別マスタ"));

            entity.HasIndex(e => e.ReviewTypeName, "M_REVIEW_TYPES_IX1").IsUnique();

            entity.HasIndex(e => e.ReviewTypeId, "M_REVIEW_TYPES_PKI").IsUnique();

            entity.Property(e => e.ReviewTypeId)
                .ValueGeneratedNever()
                .HasComment("復習種別ID")
                .HasColumnName("review_type_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("作成日時")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.ReviewTypeName)
                .HasMaxLength(20)
                .HasComment("復習種別名")
                .HasColumnName("review_type_name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<MSmallCategory>(entity =>
        {
            entity.HasKey(e => new { e.LargeId, e.SmallId }).HasName("M_SMALL_CATEGORIES_PKC");

            entity.ToTable("M_SMALL_CATEGORIES", tb => tb.HasComment("小分類マスタ"));

            entity.HasIndex(e => e.SmallCategoryName, "M_SMALL_CATEGORIES_IX1").IsUnique();

            entity.HasIndex(e => new { e.LargeId, e.SmallId }, "M_SMALL_CATEGORIES_PKI").IsUnique();

            entity.Property(e => e.LargeId)
                .HasComment("大分類ID")
                .HasColumnName("large_id");
            entity.Property(e => e.SmallId)
                .HasComment("小分類ID")
                .HasColumnName("small_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("作成日時")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.SmallCategoryName)
                .HasMaxLength(30)
                .HasComment("小分類名")
                .HasColumnName("small_category_name");
            entity.Property(e => e.SortOrder)
                .HasComment("ソート順")
                .HasColumnName("sort_order");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasMaxLength(450)
                .HasComment("ユーザーId")
                .HasColumnName("user_id");

            entity.HasOne(d => d.Large).WithMany(p => p.MSmallCategories)
                .HasForeignKey(d => d.LargeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("M_SMALL_CATEGORIES_FK1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
