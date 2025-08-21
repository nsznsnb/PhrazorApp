namespace PhrazorApp.Services
{
    public sealed class FileDownloadService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileDownloadService> _log;
        private const string RootDir = "Downloads";

        public FileDownloadService(IWebHostEnvironment env, ILogger<FileDownloadService> log)
        { _env = env; _log = log; }

        public Task<ServiceResult<List<DownloadFileItem>>> GetListAsync()
        {
            var baseDir = Path.Combine(_env.ContentRootPath ?? string.Empty, RootDir);
            var list = new List<DownloadFileItem>();
            if (Directory.Exists(baseDir))
            {
                foreach (var path in Directory.EnumerateFiles(baseDir, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        var rel = Path.GetRelativePath(baseDir, path);
                        var urlRel = string.Join('/',
                            rel.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                               .Select(Uri.EscapeDataString));

                        var fi = new FileInfo(path);
                        list.Add(new DownloadFileItem
                        {
                            FileName = fi.Name,
                            RelativePath = rel.Replace('\\', '/'),
                            ViewUrl = $"/files/{urlRel}",
                            DownloadUrl = $"/files/{urlRel}?download=1",
                            Length = fi.Length,
                            LastModified = fi.LastWriteTimeUtc,
                        });
                    }
                    catch (Exception ex)
                    {
                        _log.LogWarning(ex, "stat fail: {path}", path);
                    }
                }
                list = list.OrderBy(x => x.RelativePath, StringComparer.OrdinalIgnoreCase).ToList();
            }

            return Task.FromResult(ServiceResult.Success(list));
        }
    }

    public sealed class DownloadFileItem
    {
        public required string FileName { get; init; }
        public required string RelativePath { get; init; }
        public required string ViewUrl { get; init; }
        public required string DownloadUrl { get; init; }
        public long Length { get; init; }
        public DateTime LastModified { get; init; }
    }
}
