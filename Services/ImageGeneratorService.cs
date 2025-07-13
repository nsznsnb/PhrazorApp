using System.Net.Http.Headers;
using System.Text.Json;

namespace PhrazorApp.Services
{

    /// <summary>
    /// 画像生成サービス
    /// </summary>
    public class ImageGeneratorService 
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public ImageGeneratorService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string?> GenerateImageAsync(string prompt)
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
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("data")[0].GetProperty("url").GetString();
        }
    }
}
