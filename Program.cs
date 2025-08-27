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
using PhrazorApp.Components.Account;   // Identity �w���p
using PhrazorApp.Data;                 // ApplicationDbContext�iIdentity �p�j
using PhrazorApp.Data.UnitOfWork;      // UnitOfWork
using PhrazorApp.Endpoints;
using PhrazorApp.Models;               // ApplicationUser, DTO
using PhrazorApp.Models.Validators;    // FluentValidation �o���f�[�^
using PhrazorApp.Services;             // �h���C���T�[�r�X
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

        // ����������������������������������������������������������������������������������
        // UI �t���[�����[�N
        // ����������������������������������������������������������������������������������
        builder.Services.AddMudServices(); // MudBlazor

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents(); // Blazor Server �p�T�[�r�X
        builder.Services.AddCascadingAuthenticationState();

        builder.Services.AddServerSideBlazor()
          .AddHubOptions(o =>
          {
              o.MaximumReceiveMessageSize = 1_048_576; // �܂�1MB�B�S�z�Ȃ�512KB�ł���
              o.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
              o.KeepAliveInterval = TimeSpan.FromSeconds(15);
          });

        // ����������������������������������������������������������������������������������
        // �F�؁E�F�iIdentity + ApplicationDbContext�j
        // ����������������������������������������������������������������������������������
        var identityConn = cfg.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("�ڑ������� 'DefaultConnection' ��������܂���B");

        builder.Services.AddDbContext<ApplicationDbContext>(opt =>
           opt.UseSqlServer(identityConn, b => b.MigrationsHistoryTable("__EFMigrationsHistory_Identity")));


        builder.Services.AddIdentityCore<ApplicationUser>(opt =>
        {
            opt.SignIn.RequireConfirmedAccount = true; // ���[���m�F�K�{
            opt.User.RequireUniqueEmail = true;        // ���[���A�h���X���
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

        // ����������������������������������������������������������������������������������
        // �Ɩ� DB�iEngDbContext�j+ UnitOfWork
        // ����������������������������������������������������������������������������������
        var appConn = cfg.GetConnectionString("EngDbContext")
            ?? throw new InvalidOperationException("�ڑ������� 'EngDbContext' ��������܂���B");

        builder.Services.AddDbContextFactory<EngDbContext>(opt =>
        {
            if (env.IsDevelopment())
            {
                opt.EnableSensitiveDataLogging(); // �J�����̂ݏڍ׃��O
                opt.EnableDetailedErrors();
            }
            opt.UseSqlServer(appConn, b => b.MigrationsHistoryTable("__EFMigrationsHistory_App"));
            opt.UseSqlServer(appConn, sql => sql.CommandTimeout(120));
        });

        builder.Services.AddDatabaseDeveloperPageExceptionFilter(); // �J������ DB ��O�y�[�W
        builder.Services.AddScoped<UnitOfWork>();

        // ����������������������������������������������������������������������������������
        // �N���X�J�b�g�iHttp, Resend, Blob, HttpContext�j
        // ����������������������������������������������������������������������������������
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddHttpClient<OpenAiClient>();
        builder.Services.AddHttpClient<ResendClient>();

        // Resend�iIEmailSender�j�c API �L�[������� Resend�A������� NoOp �Ƀt�H�[���o�b�N
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


        // ---- Azure Storage�i�摜�j-------------------------
        var azSection = cfg.GetSection("AzureStorage");
        var storageConnStr = azSection.GetValue<string>("ConnectionString")
            ?? throw new InvalidOperationException("AzureStorage:ConnectionString �����ݒ�ł��B");
        var imagesContainerName = azSection.GetValue<string>("Containers:Images") ?? "phraseimage";

        // BlobServiceClient�i�C���t���p�j
        builder.Services.AddSingleton(new BlobServiceClient(storageConnStr));
        builder.Services.AddSingleton<BlobStorageClient>();

        #if DEBUG
                // WebSocket���f���O
                builder.Services.AddSingleton<CircuitHandler, DiagnosticCircuitHandler>();
        #endif

        // ����������������������������������������������������������������������������������
        // UI �w�iBlazor Server �� Circuit �P�ʂŏ�Ԃ������� Scoped�j
        // ����������������������������������������������������������������������������������
        builder.Services.AddScoped<ReviewSession>();
        builder.Services.AddScoped<TestResultSession>();
        builder.Services.AddScoped<LoadingManager>();
        builder.Services.AddScoped<JsInteropManager>();
        builder.Services.AddScoped<UiOperationRunner>();
        builder.Services.AddScoped<PageMessageStore>();
        // ProtectedSessionStorage ���g���ꍇ�̂ݗL����
        // builder.Services.AddProtectedBrowserStorage();

        // ����������������������������������������������������������������������������������
        // �h���C���T�[�r�X�i�Ɩ����W�b�N�j
        // ����������������������������������������������������������������������������������
        // �D��x��
        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<OperationLimitService>();

        // �ȉ��̊Ԃ̃T�[�r�X�Ԃ�DI�͋֎~
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



        // ����������������������������������������������������������������������������������
        // �o���f�[�V�����iFluentValidation�j
        // ����������������������������������������������������������������������������������
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




        ValidationBootstrapper.Configure(); // �A�v�����ʂ� FluentValidation �ݒ�

        // ����������������������������������������������������������������������������������
        // �I�v�V�����iOptions �p�^�[���j
        // ����������������������������������������������������������������������������������
        builder.Services.Configure<SeedUserOptions>(cfg.GetSection("SeedUser"));
        builder.Services.Configure<AzureStorageOptions>(cfg.GetSection("AzureStorage"));
        builder.Services.Configure<OpenAiOptions>(cfg.GetSection("OpenAI"));
        builder.Services.Configure<ResendOptions>(cfg.GetSection("Resend"));

        builder.Services.AddMudMarkdownServices();

        var app = builder.Build();

        // ����������������������������������������������������������������������������������
        // HTTP �p�C�v���C��
        // ����������������������������������������������������������������������������������
        if (env.IsDevelopment())
        {
            app.UseMigrationsEndPoint();


        }
        else
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts(); // ���� 30 ��
        }

        // �������[�U�쐬
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var seedOptions = services.GetRequiredService<IOptions<SeedUserOptions>>().Value;
        await SeedUserData.InitializeAsync(services, seedOptions);

        app.UseHttpsRedirection();
        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
           .AddInteractiveServerRenderMode();

        // Identity �� /Account �G���h�|�C���g
        app.MapAdditionalIdentityEndpoints();
        // �F�t���t�@�C���z�M�p�G���h�|�C���g
        app.MapFileEndpoints();
        // �w���X�`�F�b�N�p�G���h�|�C���g
        app.MapGet("/healthz", () => Results.Ok("ok"))
            .WithMetadata(new AllowAnonymousAttribute()); // �F�؂Ȃ���200

        app.Run();
    }
}
