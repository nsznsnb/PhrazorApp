using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MudBlazor.Services;
using PhraseMudBlazor.Data;
using PhrazorApp.Components;
using PhrazorApp.Components.Account;
using PhrazorApp.Data;
using PhrazorApp.Services;

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

            var connectionString = builder.Configuration.GetConnectionString("IdentityConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();


            builder.Services.AddDbContextFactory<EngDbContext>(opt =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    // 開発時は詳細なエラーとセンシティブデータのロギングを有効にする
                    opt = opt.EnableSensitiveDataLogging().EnableDetailedErrors();
                }
                opt.UseSqlServer(
                    builder.Configuration.GetConnectionString("EngDbContext"));
            });

            // ユーザーシークレットを使用（開発時のみ）
            builder.Configuration.AddUserSecrets<Program>();

            builder.Services.Configure<SeedUserOptions>(
                builder.Configuration.GetSection("SeedUser"));

            builder.Services.AddHttpClient<ImageGeneratorService>();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<ICategoryService, CategoryService>();


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
