using PhrazorApp.Constants;
using PhrazorApp.Extensions;
using System.Net.Http;

namespace PhrazorApp.Services
{
    public interface IImageService
    {
        public Task<string?> GenerateImageAsync(string prompt);
        public Task<string?> SaveImageFromUrlAsync(string prompt, string imageUrl);
        public Task<byte[]?> DownloadImageAsync(string imageUrl);


    }

    /// <summary>
    /// 画像サービス
    /// </summary>
    public class ImageService : IImageService
    {
        private readonly OpenAiClient _openAi;
        private readonly BlobStorageClient _blob;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public ImageService(OpenAiClient openAi, BlobStorageClient blob, HttpClient httpClient, ILogger<OpenAiClient> logger)
        {
            _openAi = openAi;
            _blob = blob;
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// 画像を生成します
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public async Task<string?> GenerateImageAsync(string prompt)
        {
            return await _openAi.GenerateImageUrlAsync(prompt);
        }

        /// <summary>
        /// Urlの画像をストレージに保存します
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="imageUrl"></param>
        /// <returns></returns>
        public async Task<string?> SaveImageFromUrlAsync(string prompt, string imageUrl)
        {
            var imageBytes = await DownloadImageAsync(imageUrl);
            if (imageBytes == null)
            {
                return string.Empty;
            }
            return await _blob.UploadImageAsync(prompt, imageBytes);
        }

        /// <summary>
        /// 画像をダウンロードします
        /// </summary>
        /// <param name="imageUrl">画像Url</param>
        /// <returns></returns>
        public async Task<byte[]?> DownloadImageAsync(string imageUrl)
        {
            try
            {
                return await _httpClient.GetByteArrayAsync(imageUrl);
            }
            catch (Exception ex)
            {
                _logger.LogErrorWithContext(ComLogEvents.DownloadItem, string.Format(ComMessage.MSG_E_FAILURE_DETAIL, "画像ダウンロード"));
                return null;
            }
        }
    }
}
