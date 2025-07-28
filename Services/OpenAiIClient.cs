using Azure;
using Microsoft.Extensions.Options;
using PhrazorApp.Constants;
using PhrazorApp.Extensions;
using PhrazorApp.Options;
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
        private readonly OpenAiOptions _options;

        public OpenAiClient(HttpClient httpClient, IConfiguration config, ILogger<ImageService> logger, IOptions<OpenAiOptions> options)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
            _options = options.Value;
        }


        /// <summary>
        /// 画像Urlを生成します
        /// </summary>
        /// <param name="phrase"></param>
        /// <returns></returns>
        public async Task<string?> GenerateImageUrlAsync(string phrase)
        {
            var apiKey = _options.ApiKey;
            var size = _options.ImageSize;

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var prompt = await BuildPromptAsync(phrase);

            var requestBody = new
            {
                prompt = prompt,
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
                _logger.LogErrorWithContext(ComLogEvents.GetItem, string.Format(ComMessage.MSG_E_FAILURE_DETAIL, "画像生成", $"ステータスコード({response.StatusCode})"));
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("data")[0].GetProperty("url").GetString();
        }
        
        /// <summary>
        /// フレーズから画像生成のためのプロンプトを組み立てます
        /// </summary>
        /// <param name="phrase">フレーズ</param>
        /// <returns></returns>
        public async Task<string> BuildPromptAsync(string phrase)
        {
            // 一旦GPTを挟んで、最適な画像作成指示ができるようにする
            var request = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    // 画像生成エンドポイントに最適なプロンプトになるよう指示
                    new { role = "system", content = "You convert short English sentences into detailed prompts suitable for generating illustrations using DALL·E" },
                    new { role = "user", content = $"Convert this sentence into a detailed image prompt: {phrase}" }
                },
                // 出力のランダム加減
                temperature = 0.7
            };

            var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", request);
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()!;
        }
    }

}
