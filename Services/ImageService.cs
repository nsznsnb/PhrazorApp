using PhrazorApp.Commons;
using PhrazorApp.Extensions;
using PhrazorApp.Infrastructure;
using System.Net.Http;

namespace PhrazorApp.Services
{
    /// <summary>画像サービス（OpenAI 生成／Blob 保存／HTTP ダウンロード）— CT 不使用</summary>
    public class ImageService
    {
        private readonly OpenAiClient _openAi;
        private readonly BlobStorageClient _blob;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public ImageService(
            OpenAiClient openAi,
            BlobStorageClient blob,
            HttpClient httpClient,
            ILogger<ImageService> logger)
        {
            _openAi = openAi;
            _blob = blob;
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>画像URLを生成</summary>
        public async Task<ServiceResult<string>> GenerateImageAsync(string prompt)
        {
            try
            {
                var result = await _openAi.GenerateImageUrlAsync(prompt);
                if (string.IsNullOrWhiteSpace(result.Data))
                    return ServiceResult.Error<string>("画像生成に失敗しました。");
                return ServiceResult.Success(result.Data, "画像を生成しました。");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "画像生成エラー");
                return ServiceResult.Error<string>("画像生成に失敗しました。");
            }
        }

        /// <summary>URLの画像をダウンロード</summary>
        public async Task<ServiceResult<byte[]>> DownloadImageAsync(string imageUrl)
        {
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(imageUrl);
                if (bytes is null || bytes.Length == 0)
                    return ServiceResult.Error<byte[]>("画像のダウンロードに失敗しました。");
                return ServiceResult.Success(bytes, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "画像ダウンロードエラー: {Url}", imageUrl);
                return ServiceResult.Error<byte[]>("画像ダウンロードに失敗しました。");
            }
        }

        /// <summary>URLの画像をストレージへ保存</summary>
        public async Task<ServiceResult<string>> SaveImageFromUrlAsync(string prompt, string imageUrl)
        {
            try
            {
                var dl = await DownloadImageAsync(imageUrl);
                if (!dl.IsSuccess || dl.Data is null)
                    return ServiceResult.Error<string>(dl.Message ?? "画像の取得に失敗しました。");

                var result = await _blob.UploadImageAsync(prompt, dl.Data);
                if (string.IsNullOrWhiteSpace(result.Data))
                    return ServiceResult.Error<string>("画像の保存に失敗しました。");

                return ServiceResult.Success(result.Data, "画像を保存しました。");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "画像保存エラー: {Url}", imageUrl);
                return ServiceResult.Error<string>("画像保存に失敗しました。");
            }
        }
    }
}
