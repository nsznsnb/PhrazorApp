using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using PhrazorApp.Commons;
using PhrazorApp.Extensions;
using System.Net.Http;

namespace PhrazorApp.Infrastructure
{

    /// <summary>
    /// Azure Blob ストレージ クライアント（画像アップロード用）
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
        /// <param name="ct">キャンセルトークン</param>
        public async Task<ServiceResult<string>> UploadImageAsync(
            string prompt,
            byte[] imageBytes,
            CancellationToken ct = default)
        {
            if (imageBytes is null || imageBytes.Length == 0)
                return ServiceResult.Failure<string>("画像データが空です。");

            var fileName = $"{SanitizeFileName(prompt)}_{Guid.NewGuid():N}.png";

            try
            {
                var container = _blobServiceClient.GetBlobContainerClient(_options.ContainerName);

                // コンテナがなければ作成（一般公開: Blob 単位）
                await container.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);

                var blob = container.GetBlobClient(fileName);

                using var stream = new MemoryStream(imageBytes, writable: false);

                var uploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = "image/png",
                        CacheControl = "public,max-age=31536000" // 任意：CDN/ブラウザキャッシュ
                    }
                };

                await blob.UploadAsync(stream, uploadOptions, ct);


                // 成功時メッセージは UI でそのまま表示されます（空にすれば表示なし）
                return ServiceResult.Success(blob.Uri.ToString(), $"画像を保存しました。（{fileName}）");
            }
            catch (OperationCanceledException)
            {
                // キャンセルは UI 側（UiOperationRunner）で一元処理
                throw;
            }
            catch (Exception ex)
            {


                return ServiceResult.Failure<string>("画像の保存に失敗しました。");
            }
        }

        /// <summary>
        /// ファイル名をサニタイズします（無効文字を '_'、空白を '_' に置換）。
        /// </summary>
        private static string SanitizeFileName(string fileName)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(c, '_');

            return fileName.Replace(' ', '_');
        }
    }
}
