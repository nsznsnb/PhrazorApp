using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PhrazorApp.Data.Entities;

namespace PhrazorApp.Data;

public partial class EngDbContext : DbContext
{
    public EngDbContext(DbContextOptions<EngDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DDailyUsage> DDailyUsages { get; set; }

    public virtual DbSet<DEnglishDiary> DEnglishDiarys { get; set; }

    public virtual DbSet<DEnglishDiaryTag> DEnglishDiaryTags { get; set; }

    public virtual DbSet<DPhrase> DPhrases { get; set; }

    public virtual DbSet<DPhraseImage> DPhraseImages { get; set; }

    public virtual DbSet<DReviewLog> DReviewLogs { get; set; }

    public virtual DbSet<DTestResult> DTestResults { get; set; }

    public virtual DbSet<DTestResultDetail> DTestResultDetails { get; set; }

    public virtual DbSet<MDiaryTag> MDiaryTags { get; set; }

    public virtual DbSet<MGenre> MGenres { get; set; }

    public virtual DbSet<MGrade> MGrades { get; set; }

    public virtual DbSet<MOperationType> MOperationTypes { get; set; }

    public virtual DbSet<MPhraseBook> MPhraseBooks { get; set; }

    public virtual DbSet<MPhraseBookItem> MPhraseBookItems { get; set; }

    public virtual DbSet<MPhraseGenre> MPhraseGenres { get; set; }

    public virtual DbSet<MProverb> MProverbs { get; set; }

    public virtual DbSet<MReviewType> MReviewTypes { get; set; }

    public virtual DbSet<MSubGenre> MSubGenres { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DDailyUsage>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.OperationDate, e.OperationTypeId }).HasName("D_DAILY_USAGE_PKC");

            entity.ToTable("D_DAILY_USAGE", tb => tb.HasComment("操作履歴"));

            entity.HasIndex(e => new { e.UserId, e.OperationDate, e.OperationTypeId }, "D_DAILY_USAGE_PKI").IsUnique();

            entity.Property(e => e.UserId)
                .HasComment("ユーザーID")
                .HasColumnName("user_id");
            entity.Property(e => e.OperationDate)
                .HasComment("操作日")
                .HasColumnName("operation_date");
            entity.Property(e => e.OperationTypeId)
                .HasComment("操作種別ID")
                .HasColumnName("operation_type_id");
            entity.Property(e => e.OperationCount)
                .HasComment("操作回数")
                .HasColumnName("operation_count");

            entity.HasOne(d => d.OperationType).WithMany(p => p.DDailyUsages)
                .HasForeignKey(d => d.OperationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("D_DAILY_USAGE_FK1");
        });

        modelBuilder.Entity<DEnglishDiary>(entity =>
        {
            entity.HasKey(e => e.DiaryId).HasName("D_ENGLISH_DIARYS_PKC");

            entity.ToTable("D_ENGLISH_DIARYS", tb => tb.HasComment("英語日記"));

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "D_ENGLISH_DIARYS_IX1");

            entity.HasIndex(e => new { e.UserId, e.Title }, "D_ENGLISH_DIARYS_IX2");

            entity.HasIndex(e => e.DiaryId, "D_ENGLISH_DIARYS_PKI").IsUnique();

            entity.Property(e => e.DiaryId)
                .ValueGeneratedNever()
                .HasComment("英語日記ID")
                .HasColumnName("diary_id");
            entity.Property(e => e.Content)
                .HasMaxLength(1000)
                .HasComment("内容")
                .HasColumnName("content");
            entity.Property(e => e.Correction)
                .HasMaxLength(1000)
                .HasComment("添削結果")
                .HasColumnName("correction");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("作成日時")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Explanation)
                .HasMaxLength(1000)
                .HasComment("解説")
                .HasColumnName("explanation");
            entity.Property(e => e.Note)
                .HasMaxLength(1000)
                .HasComment("補足")
                .HasColumnName("note");
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
                .HasComment("ユーザーId")
                .HasColumnName("user_id");
        });

        modelBuilder.Entity<DEnglishDiaryTag>(entity =>
        {
            entity.HasKey(e => new { e.DiaryId, e.DiaryTagId }).HasName("D_ENGLISH_DIARY_TAGS_PKC");

            entity.ToTable("D_ENGLISH_DIARY_TAGS", tb => tb.HasComment("英語日記タグ"));

            entity.HasIndex(e => e.DiaryId, "D_ENGLISH_DIARY_TAGS_IX1");

            entity.HasIndex(e => e.DiaryTagId, "D_ENGLISH_DIARY_TAGS_IX2");

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

        modelBuilder.Entity<DPhrase>(entity =>
        {
            entity.HasKey(e => e.PhraseId).HasName("D_PHRASES_PKC");

            entity.ToTable("D_PHRASES", tb => tb.HasComment("ユーザーフレーズ"));

            entity.HasIndex(e => e.UserId, "D_PHRASES_IX1");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "D_PHRASES_IX2").IsDescending(false, true);

            entity.HasIndex(e => e.PhraseId, "D_PHRASES_PKI").IsUnique();

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
                .HasComment("ユーザーID")
                .HasColumnName("user_id");
        });

        modelBuilder.Entity<DPhraseImage>(entity =>
        {
            entity.HasKey(e => e.PhraseImageId).HasName("D_PHRASE_IMAGES_PKC");

            entity.ToTable("D_PHRASE_IMAGES", tb => tb.HasComment("フレーズ画像"));

            entity.HasIndex(e => e.PhraseId, "D_PHRASE_IMAGES_IX1").IsUnique();

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

            entity.HasOne(d => d.Phrase).WithOne(p => p.DPhraseImage)
                .HasForeignKey<DPhraseImage>(d => d.PhraseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("D_PHRASE_IMAGES_FK1");
        });

        modelBuilder.Entity<DReviewLog>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("D_REVIEW_LOGS_PKC");

            entity.ToTable("D_REVIEW_LOGS", tb => tb.HasComment("復習履歴"));

            entity.HasIndex(e => new { e.PhraseId, e.ReviewDate }, "D_REVIEW_LOGS_IX1");

            entity.HasIndex(e => new { e.TestId, e.TestResultDetailNo }, "D_REVIEW_LOGS_IX2");

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

        modelBuilder.Entity<DTestResult>(entity =>
        {
            entity.HasKey(e => e.TestId).HasName("D_TEST_RESULTS_PKC");

            entity.ToTable("D_TEST_RESULTS", tb => tb.HasComment("テスト結果"));

            entity.HasIndex(e => new { e.UserId, e.TestDatetime }, "D_TEST_RESULTS_IX1");

            entity.HasIndex(e => e.GradeId, "D_TEST_RESULTS_IX2");

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
            entity.Property(e => e.UserId)
                .HasComment("ユーザーID")
                .HasColumnName("user_id");

            entity.HasOne(d => d.Grade).WithMany(p => p.DTestResults)
                .HasForeignKey(d => d.GradeId)
                .HasConstraintName("D_TEST_RESULTS_FK1");
        });

        modelBuilder.Entity<DTestResultDetail>(entity =>
        {
            entity.HasKey(e => new { e.TestId, e.TestResultDetailNo }).HasName("D_TEST_RESULT_DETAILS_PKC");

            entity.ToTable("D_TEST_RESULT_DETAILS", tb => tb.HasComment("テスト結果明細"));

            entity.HasIndex(e => e.PhraseId, "D_TEST_RESULT_DETAILS_IX1");

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

        modelBuilder.Entity<MDiaryTag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("M_DIARY_TAGS_PKC");

            entity.ToTable("M_DIARY_TAGS", tb => tb.HasComment("日記タグ"));

            entity.HasIndex(e => new { e.UserId, e.TagName }, "M_DIARY_TAGS_IX1").IsUnique();

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
            entity.Property(e => e.UserId)
                .HasComment("ユーザーID")
                .HasColumnName("user_id");
        });

        modelBuilder.Entity<MGenre>(entity =>
        {
            entity.HasKey(e => e.GenreId).HasName("M_GENRES_PKC");

            entity.ToTable("M_GENRES", tb => tb.HasComment("ジャンルマスタ"));

            entity.HasIndex(e => new { e.UserId, e.GenreName }, "M_GENRES_IX1").IsUnique();

            entity.HasIndex(e => e.GenreId, "M_GENRES_PKI").IsUnique();

            entity.Property(e => e.GenreId)
                .ValueGeneratedNever()
                .HasComment("ジャンルID")
                .HasColumnName("genre_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("作成日時")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.GenreName)
                .HasMaxLength(30)
                .HasComment("ジャンル名")
                .HasColumnName("genre_name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasComment("ユーザーId")
                .HasColumnName("user_id");
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

        modelBuilder.Entity<MOperationType>(entity =>
        {
            entity.HasKey(e => e.OperationTypeId).HasName("M_OPERATION_TYPES_PKC");

            entity.ToTable("M_OPERATION_TYPES", tb => tb.HasComment("操作種別マスタ"));

            entity.HasIndex(e => e.OperationTypeName, "M_OPERATION_TYPES_IX1").IsUnique();

            entity.HasIndex(e => e.OperationTypeId, "M_OPERATION_TYPES_PKI").IsUnique();

            entity.Property(e => e.OperationTypeId)
                .ValueGeneratedNever()
                .HasComment("操作種別ID")
                .HasColumnName("operation_type_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("作成日時")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.OperationTypeCode)
                .HasMaxLength(50)
                .HasComment("操作種別コード")
                .HasColumnName("operation_type_code");
            entity.Property(e => e.OperationTypeLimit)
                .HasComment("操作回数上限")
                .HasColumnName("operation_type_limit");
            entity.Property(e => e.OperationTypeName)
                .HasMaxLength(20)
                .HasComment("操作種別名")
                .HasColumnName("operation_type_name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<MPhraseBook>(entity =>
        {
            entity.HasKey(e => e.PhraseBookId).HasName("M_PHRASE_BOOKS_PKC");

            entity.ToTable("M_PHRASE_BOOKS", tb => tb.HasComment("フレーズ帳マスタ"));

            entity.HasIndex(e => new { e.UserId, e.PhraseBookName }, "M_PHRASE_BOOKS_IX1").IsUnique();

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
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasComment("説明")
                .HasColumnName("description");
            entity.Property(e => e.PhraseBookName)
                .HasMaxLength(50)
                .HasComment("フレーズ帳名")
                .HasColumnName("phrase_book_name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasComment("ユーザーId")
                .HasColumnName("user_id");
        });

        modelBuilder.Entity<MPhraseBookItem>(entity =>
        {
            entity.HasKey(e => new { e.PhraseBookId, e.PhraseId }).HasName("M_PHRASE_BOOK_ITEMS_PKC");

            entity.ToTable("M_PHRASE_BOOK_ITEMS", tb => tb.HasComment("フレーズ帳アイテム"));

            entity.HasIndex(e => new { e.PhraseBookId, e.OrderNo }, "M_PHRASE_BOOK_ITEMS_IX1");

            entity.HasIndex(e => new { e.PhraseBookId, e.PhraseId }, "M_PHRASE_BOOK_ITEMS_PKI").IsUnique();

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
            entity.Property(e => e.Note)
                .HasMaxLength(100)
                .HasComment("メモ")
                .HasColumnName("note");
            entity.Property(e => e.OrderNo)
                .HasComment("ソート順")
                .HasColumnName("order_no");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.PhraseBook).WithMany(p => p.MPhraseBookItems)
                .HasForeignKey(d => d.PhraseBookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("M_PHRASE_BOOK_ITEMS_FK1");

            entity.HasOne(d => d.Phrase).WithMany(p => p.MPhraseBookItems)
                .HasForeignKey(d => d.PhraseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("M_PHRASE_BOOK_ITEMS_FK2");
        });

        modelBuilder.Entity<MPhraseGenre>(entity =>
        {
            entity.HasKey(e => new { e.PhraseId, e.SubGenreId, e.GenreId }).HasName("M_PHRASE_GENRES_PKC");

            entity.ToTable("M_PHRASE_GENRES", tb => tb.HasComment("フレーズジャンル"));

            entity.HasIndex(e => e.PhraseId, "M_PHRASE_GENRES_IX1");

            entity.HasIndex(e => new { e.GenreId, e.SubGenreId }, "M_PHRASE_GENRES_IX2");

            entity.HasIndex(e => new { e.PhraseId, e.SubGenreId, e.GenreId }, "M_PHRASE_GENRES_PKI").IsUnique();

            entity.Property(e => e.PhraseId)
                .HasComment("フレーズID")
                .HasColumnName("phrase_id");
            entity.Property(e => e.SubGenreId)
                .HasComment("サブジャンルID")
                .HasColumnName("sub_genre_id");
            entity.Property(e => e.GenreId)
                .HasComment("ジャンルID")
                .HasColumnName("genre_id");
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

            entity.HasOne(d => d.Phrase).WithMany(p => p.MPhraseGenres)
                .HasForeignKey(d => d.PhraseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("M_PHRASE_GENRES_FK1");

            entity.HasOne(d => d.MSubGenre).WithMany(p => p.MPhraseGenres)
                .HasForeignKey(d => new { d.GenreId, d.SubGenreId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("M_PHRASE_GENRES_FK2");
        });

        modelBuilder.Entity<MProverb>(entity =>
        {
            entity.HasKey(e => e.ProverbId).HasName("M_PROVERBS_PKC");

            entity.ToTable("M_PROVERBS", tb => tb.HasComment("格言マスタ"));

            entity.HasIndex(e => e.ProverbId, "M_PROVERBS_PKI").IsUnique();

            entity.Property(e => e.ProverbId)
                .ValueGeneratedNever()
                .HasComment("格言ID")
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

        modelBuilder.Entity<MSubGenre>(entity =>
        {
            entity.HasKey(e => new { e.GenreId, e.SubGenreId }).HasName("M_SUB_GENRES_PKC");

            entity.ToTable("M_SUB_GENRES", tb => tb.HasComment("サブジャンルマスタ"));

            entity.HasIndex(e => new { e.UserId, e.GenreId, e.SubGenreName }, "M_SUB_GENRES_IX1").IsUnique();

            entity.HasIndex(e => new { e.UserId, e.GenreId }, "M_SUB_GENRES_IX_DEFAULT")
                .IsUnique()
                .HasFilter("([default_flg]=(1))");

            entity.HasIndex(e => new { e.GenreId, e.SubGenreId }, "M_SUB_GENRES_PKI").IsUnique();

            entity.Property(e => e.GenreId)
                .HasComment("ジャンルID")
                .HasColumnName("genre_id");
            entity.Property(e => e.SubGenreId)
                .HasComment("サブジャンルID")
                .HasColumnName("sub_genre_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("作成日時")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DefaultFlg)
                .HasComment("デフォルトフラグ")
                .HasColumnName("default_flg");
            entity.Property(e => e.OrderNo)
                .HasComment("ソート順")
                .HasColumnName("order_no");
            entity.Property(e => e.SubGenreName)
                .HasMaxLength(30)
                .HasComment("サブジャンル名")
                .HasColumnName("sub_genre_name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasComment("更新日時")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasComment("ユーザーId")
                .HasColumnName("user_id");

            entity.HasOne(d => d.Genre).WithMany(p => p.MSubGenres)
                .HasForeignKey(d => d.GenreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("M_SUB_GENRES_FK1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
