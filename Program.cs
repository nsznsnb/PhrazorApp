using Azure.Storage.Blobs;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MudBlazor;
using MudBlazor.Services;
using PhrazorApp.Components;           // App
using PhrazorApp.Components.Account;   // Identity ヘルパ
using PhrazorApp.Data;                 // ApplicationDbContext（Identity 用）
using PhrazorApp.Data.UnitOfWork;      // UnitOfWork
using PhrazorApp.Endpoints;
using PhrazorApp.Models;               // ApplicationUser, DTO
using PhrazorApp.Models.Validators;    // FluentValidation バリデータ
using PhrazorApp.Services;             // ドメインサービス
using PhrazorApp.UI.Interop;           // JsInteropManager
using PhrazorApp.UI.Managers;          // LoadingManager, UiOperationRunner
using PhrazorApp.UI.State;             // ReviewSession, TestResultSession, PageMessageStore
using PhrazorApp.Validators;
using Resend;

namespace PhrazorApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var cfg = builder.Configuration;
        var env = builder.Environment;

        // ─────────────────────────────────────────
        // UI フレームワーク
        // ─────────────────────────────────────────
        builder.Services.AddMudServices(); // MudBlazor

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents(); // Blazor Server 用サービス
        builder.Services.AddCascadingAuthenticationState();

        builder.Services.AddServerSideBlazor()
          .AddHubOptions(o =>
          {
              o.MaximumReceiveMessageSize = 1_048_576; // まず1MB。心配なら512KBでも可
              o.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
              o.KeepAliveInterval = TimeSpan.FromSeconds(15);
          });

        // ─────────────────────────────────────────
        // 認証・認可（Identity + ApplicationDbContext）
        // ─────────────────────────────────────────
        var identityConn = cfg.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("接続文字列 'DefaultConnection' が見つかりません。");

        builder.Services.AddDbContext<ApplicationDbContext>(opt =>
           opt.UseSqlServer(identityConn, b => b.MigrationsHistoryTable("__EFMigrationsHistory_Identity")));


        builder.Services.AddIdentityCore<ApplicationUser>(opt =>
        {
            opt.SignIn.RequireConfirmedAccount = true; // メール確認必須
            opt.User.RequireUniqueEmail = true;        // メールアドレス一意
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddErrorDescriber<JapaneseIdentityErrorDescriber>()
            .AddDefaultTokenProviders();

        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
            .AddIdentityCookies();

        // ─────────────────────────────────────────
        // 業務 DB（EngDbContext）+ UnitOfWork
        // ─────────────────────────────────────────
        var appConn = cfg.GetConnectionString("EngDbContext")
            ?? throw new InvalidOperationException("接続文字列 'EngDbContext' が見つかりません。");

        builder.Services.AddDbContextFactory<EngDbContext>(opt =>
        {
            if (env.IsDevelopment())
            {
                opt.EnableSensitiveDataLogging(); // 開発時のみ詳細ログ
                opt.EnableDetailedErrors();
            }
            opt.UseSqlServer(appConn, b => b.MigrationsHistoryTable("__EFMigrationsHistory_App"));
            opt.UseSqlServer(appConn, sql => sql.CommandTimeout(120));
        });

        builder.Services.AddDatabaseDeveloperPageExceptionFilter(); // 開発時の DB 例外ページ
        builder.Services.AddScoped<UnitOfWork>();

        // ─────────────────────────────────────────
        // クロスカット（Http, Resend, Blob, HttpContext）
        // ─────────────────────────────────────────
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddHttpClient<OpenAiClient>();
        builder.Services.AddHttpClient<ResendClient>();

        // Resend（IEmailSender）… API キーがあれば Resend、無ければ NoOp にフォールバック
        var resendApiKey = cfg["Resend:ApiKey"];
        if (!string.IsNullOrWhiteSpace(resendApiKey))
        {
            builder.Services.Configure<ResendClientOptions>(o => o.ApiToken = resendApiKey);
            builder.Services.AddScoped<IResend, ResendClient>();
            builder.Services.AddScoped<IEmailSender<ApplicationUser>, ResendEmailSender>();
        }
        else
        {
            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
        }


        // ---- Azure Storage（画像）-------------------------
        var azSection = cfg.GetSection("AzureStorage");
        var storageConnStr = azSection.GetValue<string>("ConnectionString")
            ?? throw new InvalidOperationException("AzureStorage:ConnectionString が未設定です。");
        var imagesContainerName = azSection.GetValue<string>("Containers:Images") ?? "phraseimage";

        // BlobServiceClient（インフラ用）
        builder.Services.AddSingleton(new BlobServiceClient(storageConnStr));
        builder.Services.AddSingleton<BlobStorageClient>();

        #if DEBUG
                // WebSocket寸断ログ
                builder.Services.AddSingleton<CircuitHandler, DiagnosticCircuitHandler>();
        #endif

        // ─────────────────────────────────────────
        // UI 層（Blazor Server の Circuit 単位で状態を持つため Scoped）
        // ─────────────────────────────────────────
        builder.Services.AddScoped<ReviewSession>();
        builder.Services.AddScoped<TestResultSession>();
        builder.Services.AddScoped<LoadingManager>();
        builder.Services.AddScoped<JsInteropManager>();
        builder.Services.AddScoped<UiOperationRunner>();
        builder.Services.AddScoped<PageMessageStore>();
        // ProtectedSessionStorage を使う場合のみ有効化
        // builder.Services.AddProtectedBrowserStorage();

        // ─────────────────────────────────────────
        // ドメインサービス（業務ロジック）
        // ─────────────────────────────────────────
        // 優先度高
        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<OperationLimitService>();

        // 以下の間のサービス間のDIは禁止
        builder.Services.AddScoped<ImageService>();
        builder.Services.AddScoped<HomeDashboardService>();
        builder.Services.AddScoped<GenreService>();
        builder.Services.AddScoped<PhraseService>();
        builder.Services.AddScoped<PhraseBookService>();
        builder.Services.AddScoped<TestResultService>();
        builder.Services.AddScoped<ProverbService>();
        builder.Services.AddScoped<GradeService>();
        builder.Services.AddScoped<OperationTypeService>();
        builder.Services.AddScoped<ReviewTypeService>();
        builder.Services.AddScoped<DiaryTagService>();
        builder.Services.AddScoped<EnglishDiaryService>();
        builder.Services.AddScoped<FileDownloadService>();



        // ─────────────────────────────────────────
        // バリデーション（FluentValidation）
        // ─────────────────────────────────────────
        builder.Services.AddScoped<IValidator<GenreModel>, GenreModelValidator>();
        builder.Services.AddScoped<IValidator<SubGenreModel>, SubGenreModelValidator>();
        builder.Services.AddScoped<IValidator<FileModel>, FileModelValidator>();
        builder.Services.AddScoped<IValidator<DiaryTagModel>, DiaryTagModelValidator>();
        builder.Services.AddScoped<IValidator<GradeModel>, GradeModelValidator>();
        builder.Services.AddScoped<IValidator<OperationTypeModel>, OperationTypeModelValidator>();
        builder.Services.AddScoped<IValidator<ProverbModel>, ProverbModelValidator>();
        builder.Services.AddScoped<IValidator<ReviewTypeModel>, ReviewTypeModelValidator>();
        builder.Services.AddScoped<IValidator<EnglishDiaryModel>, EnglishDiaryModelValidator>();
        builder.Services.AddScoped<IValidator<PhraseEditModel>, PhraseEditModelValidator>();




        ValidationBootstrapper.Configure(); // アプリ共通の FluentValidation 設定

        // ─────────────────────────────────────────
        // オプション（Options パターン）
        // ─────────────────────────────────────────
        builder.Services.Configure<SeedUserOptions>(cfg.GetSection("SeedUser"));
        builder.Services.Configure<AzureStorageOptions>(cfg.GetSection("AzureStorage"));
        builder.Services.Configure<OpenAiOptions>(cfg.GetSection("OpenAI"));
        builder.Services.Configure<ResendOptions>(cfg.GetSection("Resend"));

        builder.Services.AddMudMarkdownServices();

        var app = builder.Build();

        // ─────────────────────────────────────────
        // HTTP パイプライン
        // ─────────────────────────────────────────
        if (env.IsDevelopment())
        {
            app.UseMigrationsEndPoint();


        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts(); // 既定 30 日
        }

        // 初期ユーザ作成
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var seedOptions = services.GetRequiredService<IOptions<SeedUserOptions>>().Value;
        await SeedUserData.InitializeAsync(services, seedOptions);

        app.UseHttpsRedirection();
        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
           .AddInteractiveServerRenderMode();

        // Identity の /Account エンドポイント
        app.MapAdditionalIdentityEndpoints();
        // 認可付きファイル配信用エンドポイント
        app.MapFileEndpoints();
        // ヘルスチェック用エンドポイント
        app.MapGet("/healthz", () => Results.Ok("ok"))
            .WithMetadata(new AllowAnonymousAttribute()); // 認証なしで200

        app.Run();
    }
}
