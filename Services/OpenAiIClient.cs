using Azure;
using PhrazorApp.Common;
using PhrazorApp.Extensions;
using System.Net.Http.Headers;
using System.Text.Json;

namespace PhrazorApp.Services
{

    /// <summary>
    /// OpenAIクライアント
    /// </summary>
    public class OpenAiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<ImageService> _logger;

        public OpenAiClient(HttpClient httpClient, IConfiguration config, ILogger<ImageService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }


        /// <summary>
        /// 画像Urlを生成します
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public async Task<string?> GenerateImageUrlAsync(string prompt)
        {
            var apiKey = _config["OpenAI:ApiKey"];
            var size = _config["OpenAI:ImageSize"] ?? "256x256";

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var requestBody = new
            {
                prompt = prompt + ", cartoon illustration",
                n = 1,
                size = size
            };

            // 画像生成エンドポイント
            var response = await _httpClient.PostAsJsonAsync(
                "https://api.openai.com/v1/images/generations",
                requestBody
             );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogErrorWithContext(ComLogEvents.GetItem, string.Format(ComMessage.MSG_E_ERROR_EXECUTE_DETAIL, "画像生成", $"ステータスコード({response.StatusCode})"));
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("data")[0].GetProperty("url").GetString();
        }


    }

}
