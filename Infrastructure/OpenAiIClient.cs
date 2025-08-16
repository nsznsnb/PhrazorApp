using Microsoft.Extensions.Options;
using PhrazorApp.Extensions;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PhrazorApp.Infrastructure
{

    public sealed class OpenAiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenAiClient> _logger;
        private readonly OpenAiOptions _options; // ApiKey, ImageSize などを保持する Options

        public OpenAiClient(HttpClient httpClient, ILogger<OpenAiClient> logger, IOptions<OpenAiOptions> options)
        {
            _httpClient = httpClient;
            _logger = logger;
            _options = options.Value;
        }

        /// <summary>
        /// 画像 URL を生成します（DALL·E Images API）
        /// </summary>
        public async Task<ServiceResult<string>> GenerateImageUrlAsync(string phrase, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(phrase))
                return ServiceResult.Error<string>("フレーズが空です。");

            // まずプロンプトを生成
            var promptResult = await BuildPromptAsync(phrase, ct);
            if (!promptResult.IsSuccess || string.IsNullOrWhiteSpace(promptResult.Data))
                return ServiceResult.Error<string>(promptResult.Message ?? "画像プロンプトの生成に失敗しました。");

            var apiKey = _options.ApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
                return ServiceResult.Error<string>("OpenAI API キーが設定されていません。");

            var size = string.IsNullOrWhiteSpace(_options.ImageSize) ? "1024x1024" : _options.ImageSize;

            var requestBody = new
            {
                prompt = promptResult.Data,
                n = 1,
                size
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/images/generations")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            try
            {
                using var res = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
                var json = await res.Content.ReadAsStringAsync(ct);

                if (!res.IsSuccessStatusCode)
                {

                    return ServiceResult.Error<string>($"画像生成に失敗しました。（{(int)res.StatusCode}）");
                }

                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("data", out var dataArr) &&
                    dataArr.ValueKind == JsonValueKind.Array &&
                    dataArr.GetArrayLength() > 0 &&
                    dataArr[0].TryGetProperty("url", out var urlProp))
                {
                    var url = urlProp.GetString();
                    if (!string.IsNullOrWhiteSpace(url))
                        return ServiceResult.Success(url!, "画像を生成しました。");
                }

                return ServiceResult.Error<string>("画像生成レスポンスの解析に失敗しました。");
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogErrorWithContext(LogEvents.GetItem, ex, "画像生成で例外が発生しました。");
                return ServiceResult.Error<string>("画像生成に失敗しました。");
            }
        }

        /// <summary>
        /// フレーズから画像生成向けプロンプトを作成（Chat Completions API）
        /// </summary>
        public async Task<ServiceResult<string>> BuildPromptAsync(string phrase, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(phrase))
                return ServiceResult.Error<string>("フレーズが空です。");

            var apiKey = _options.ApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
                return ServiceResult.Error<string>("OpenAI API キーが設定されていません。");

            var request = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                new { role = "system", content = "You convert short English sentences into detailed prompts suitable for generating illustrations using DALL·E" },
                new { role = "user",   content = $"Convert this sentence into a detailed image prompt: {phrase}" }
            },
                temperature = 0.7
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            try
            {
                using var res = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
                var json = await res.Content.ReadAsStringAsync(ct);

                if (!res.IsSuccessStatusCode)
                {

                    return ServiceResult.Error<string>($"プロンプト生成に失敗しました。（{(int)res.StatusCode}）");
                }

                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("choices", out var choices) &&
                    choices.ValueKind == JsonValueKind.Array &&
                    choices.GetArrayLength() > 0)
                {
                    var content = choices[0].GetProperty("message").GetProperty("content").GetString();
                    if (!string.IsNullOrWhiteSpace(content))
                        return ServiceResult.Success(content!, "");
                }

                return ServiceResult.Error<string>("プロンプト生成レスポンスの解析に失敗しました。");
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _logger.LogErrorWithContext(LogEvents.GetItem, ex, "プロンプト生成で例外が発生しました。");
                return ServiceResult.Error<string>("プロンプト生成に失敗しました。");
            }
        }
    }

}
