using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using PhrazorApp.Commons;
using PhrazorApp.Extensions;

namespace PhrazorApp.Infrastructure
{
    /// <summary>
    /// Azure Blob ストレージ クライアント（画像アップロード用）— CT 不使用
    /// </summary>
    public sealed class BlobStorageClient
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly AzureBlobOptions _options;
        private readonly ILogger<BlobStorageClient> _logger;

        public BlobStorageClient(
            BlobServiceClient blobServiceClient,
            IOptions<AzureBlobOptions> options,
            ILogger<BlobStorageClient> logger)
        {
            _blobServiceClient = blobServiceClient;
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// 画像を <see cref="AzureBlobOptions.ContainerName"/> へ PNG としてアップロードします。
        /// </summary>
        /// <param name="prompt">保存名のプレフィックスに使うテキスト（サニタイズされます）</param>
        /// <param name="imageBytes">画像バイト列（PNG想定）</param>
        public async Task<ServiceResult<string>> UploadImageAsync(string prompt, byte[] imageBytes)
        {
            if (imageBytes is null || imageBytes.Length == 0)
                return ServiceResult.Error<string>("画像データが空です。");

            var fileName = $"{SanitizeFileName(prompt)}_{Guid.NewGuid():N}.png";

            try
            {
                var container = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);

                // コンテナがなければ作成（一般公開: Blob 単位）
                await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var blob = container.GetBlobClient(fileName);

                using var stream = new MemoryStream(imageBytes, writable: false);

                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = "image/png",
                        CacheControl = "public,max-age=31536000" // 任意：長期キャッシュ
                    }
                };

                await blob.UploadAsync(stream, uploadOptions);

                // 成功
                return ServiceResult.Success(blob.Uri.ToString(), $"画像を保存しました。（{fileName}）");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "画像の保存に失敗しました。FileName={FileName}, Container={Container}",
                                 fileName, _options.ContainerName);
                return ServiceResult.Error<string>("画像の保存に失敗しました。");
            }
        }

        /// <summary>
        /// ファイル名をサニタイズします（無効文字/空白を '_' に置換）。
        /// </summary>
        private static string SanitizeFileName(string fileName)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(c, '_');

            return fileName.Replace(' ', '_');
        }
    }
}
