using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;

namespace PhrazorApp.Endpoints
{
    public static class FileEndpoints
    {
        private const string Root = "Downloads"; // プロジェクト直下

        public static IEndpointRouteBuilder MapFileEndpoints(this IEndpointRouteBuilder routes)
        {
            var g = routes.MapGroup("/files").RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });

            // サブフォルダ対応（catch-all）
            g.MapGet("/{*path}", (HttpContext http, string? path, IWebHostEnvironment env) =>
            {
                if (string.IsNullOrWhiteSpace(path)) return Results.NotFound();

                var baseDir = Path.Combine(env.ContentRootPath, Root);
                var full = Path.GetFullPath(Path.Combine(baseDir, path));

                // パストラバーサル対策：必ず baseDir 配下であること
                if (!full.StartsWith(baseDir, StringComparison.Ordinal)) return Results.NotFound();
                if (!System.IO.File.Exists(full)) return Results.NotFound();

                var name = Path.GetFileName(full);

                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(name, out var contentType))
                    contentType = "application/octet-stream";

                // 既定=inline、?download=1 or ?mode=download なら attachment
                var mode = http.Request.Query["mode"].ToString();
                var isDownload = http.Request.Query.ContainsKey("download")
                                 || string.Equals(mode, "download", StringComparison.OrdinalIgnoreCase);

                var stream = System.IO.File.OpenRead(full);

                return isDownload
                    ? Results.File(stream, contentType, fileDownloadName: name, enableRangeProcessing: true)
                    : Results.File(stream, contentType, enableRangeProcessing: true);
            })
            .WithName("FilesGet");

            return routes;
        }
    }
}
