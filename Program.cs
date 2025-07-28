using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MudBlazor.Services;
using PhrazorApp.Commons;
using PhrazorApp.Components;
using PhrazorApp.Components.Account;
using PhrazorApp.Data;
using PhrazorApp.Data.Repositories;
using PhrazorApp.Options;
using PhrazorApp.Services;
using Resend;

namespace PhrazorApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add MudBlazor services
            builder.Services.AddMudServices();

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityUserAccessor>();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
                })
                .AddIdentityCookies();

            var connectionString = builder.Configuration.GetConnectionString("EngDbContext") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.User.RequireUniqueEmail = true; 
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddErrorDescriber<JapaneseIdentityErrorDescriber>()
            .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();


            builder.Services.AddDbContextFactory<EngDbContext>(opt =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    // �J�����͏ڍׂȃG���[�ƃZ���V�e�B�u�f�[�^�̃��M���O��L���ɂ���
                    opt = opt.EnableSensitiveDataLogging().EnableDetailedErrors();
                }
                opt.UseSqlServer(
                    builder.Configuration.GetConnectionString("EngDbContext"));
            });

            // Repositories�̓o�^
            builder.Services.AddScoped<IPhraseRepository, PhraseRepository>();
            builder.Services.AddScoped<IGenreRepository, GenreRepository>();

            // ���[�U�[�V�[�N���b�g���g�p�i�J�����̂݁j
            //builder.Configuration.AddUserSecrets<Program>();


            // Http�֘A
            builder.Services.AddHttpClient<OpenAiClient>();
            builder.Services.AddHttpClient<ResendClient>();
            builder.Services.AddHttpContextAccessor();


            // Resend�֘A
            var resendApiToken = builder.Configuration["Resend:ApiKey"]!;
            builder.Services.Configure<ResendClientOptions>(o =>
            {
                o.ApiToken = resendApiToken;
            });
            builder.Services.AddScoped<IResend, ResendClient>();
            builder.Services.AddScoped<IEmailSender<ApplicationUser>, ResendEmailSender>();

            // BlobStorase�֘A
            builder.Services.AddSingleton(x =>
            {
                var config = x.GetRequiredService<IConfiguration>();
                var connectionString = config["AzureBlob:ConnectionString"];
                return new BlobServiceClient(connectionString);
            });
            builder.Services.AddSingleton<BlobStorageClient>();


            builder.Services.AddScoped<LoadingService>();
            builder.Services.AddScoped<ScriptService>();
            // OpenAiClient��BlogClient��DI���Ă��邽��DI�̏����֌W�ɒ���
            builder.Services.AddScoped<IImageService, ImageService>();
            builder.Services.AddScoped<IGenreService, GenreService>();
            builder.Services.AddScoped<IPhraseService, PhraseService>();


            // �I�v�V�����p�^�[��
            builder.Services.Configure<SeedUserOptions>(builder.Configuration.GetSection("SeedUser"));
            builder.Services.Configure<AzureBlobOptions>(builder.Configuration.GetSection("AzureBlob"));
            builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection("OpenAI"));
            builder.Services.Configure<ResendOptions>(builder.Configuration.GetSection("Resend"));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();

                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;

                    var seedOptions = services.GetRequiredService<IOptions<SeedUserOptions>>().Value;

                    await SeedUserData.InitializeAsync(services, seedOptions);
                }
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Common��HttpContextAccessor���Z�b�g
            var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
            Common.SetHttpContextAccessor(httpContextAccessor);

            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            app.Run();
        }
    }
}
