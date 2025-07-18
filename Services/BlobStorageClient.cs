using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using PhrazorApp.Common;
using PhrazorApp.Extensions;
using System.Net.Http;

namespace PhrazorApp.Services
{

    /// <summary>
    /// ストレージクライアント
    /// </summary>
    public class BlobStorageClient
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BlobStorageClient> _logger;

        public BlobStorageClient(BlobServiceClient blobServiceClient, IConfiguration configuration, ILogger<BlobStorageClient> logger)
        {
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// 画像をストレージにアップロードします
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="imageBytes"></param>
        /// <returns></returns>
        public async Task<string?> UploadImageAsync(string prompt, byte[] imageBytes)
        {
            // 保存ファイル名
            var fileName = $"{SanitizeFileName(prompt)}_{Guid.NewGuid()}.png";
            try
            {
                // コンテナ名
                var containerName = _configuration["AzureBlob:ContainerName"] ?? "images";


                // コンテナクライアント取得
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var blobClient = containerClient.GetBlobClient(fileName);
                using var stream = new MemoryStream(imageBytes);

                // png画像としてアップロード
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = "image/png" });

                _logger.LogInformationWithContext(ComLogEvents.UploadItem, string.Format(ComMessage.MSG_I_SUCCESS_UPLOAD_DETAIL, fileName));
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithContext(ComLogEvents.UploadItem, ex, string.Format(ComMessage.MSG_I_SUCCESS_UPLOAD_DETAIL, fileName));
                return null;
            }
        }

        /// <summary>
        /// ファイル名をサニタイズします
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string SanitizeFileName(string fileName)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName.Replace(" ", "_");

        }


    }
}
