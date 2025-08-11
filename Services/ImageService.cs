using PhrazorApp.Commons;
using PhrazorApp.Extensions;
using PhrazorApp.Infrastructure;
using System.Net.Http;

namespace PhrazorApp.Services
{

 /// <summary>画像サービス（OpenAI 生成／Blob 保存／HTTP ダウンロード）</summary>
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
            ILogger<ImageService> logger)  // ← 型合わせ
        {
            _openAi = openAi;
            _blob = blob;
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>画像URLを生成</summary>
        public async Task<ServiceResult<string>> GenerateImageAsync(string prompt, CancellationToken ct = default)
        {
            try
            {
                var result = await _openAi.GenerateImageUrlAsync(prompt, ct);
                if (string.IsNullOrWhiteSpace(result.Data))
                    return ServiceResult.Failure<string>("画像生成に失敗しました。");
                return ServiceResult.Success(result.Data, "画像を生成しました。");
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "画像生成エラー");
                return ServiceResult.Failure<string>("画像生成に失敗しました。");
            }
        }

        /// <summary>URLの画像をダウンロード</summary>
        public async Task<ServiceResult<byte[]>> DownloadImageAsync(string imageUrl, CancellationToken ct = default)
        {
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(imageUrl, ct);
                if (bytes is null || bytes.Length == 0)
                    return ServiceResult.Failure<byte[]>("画像のダウンロードに失敗しました。");
                return ServiceResult.Success(bytes, "");
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "画像ダウンロードエラー: {Url}", imageUrl);
                return ServiceResult.Failure<byte[]>("画像ダウンロードに失敗しました。");
            }
        }

        /// <summary>URLの画像をストレージへ保存</summary>
        public async Task<ServiceResult<string>> SaveImageFromUrlAsync(string prompt, string imageUrl, CancellationToken ct = default)
        {
            try
            {
                var dl = await DownloadImageAsync(imageUrl, ct);
                if (!dl.IsSuccess || dl.Data is null)
                    return ServiceResult.Failure<string>(dl.Message ?? "画像の取得に失敗しました。");

                var result = await _blob.UploadImageAsync(prompt, dl.Data, ct);
                if (string.IsNullOrWhiteSpace(result.Data))
                    return ServiceResult.Failure<string>("画像の保存に失敗しました。");

                return ServiceResult.Success(result.Data, "画像を保存しました。");
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "画像保存エラー: {Url}", imageUrl);
                return ServiceResult.Failure<string>("画像保存に失敗しました。");
            }
        }
    }
}
