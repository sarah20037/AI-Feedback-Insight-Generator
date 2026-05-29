using FeedbackAPI.Models;
using FeedbackAPI.Prompts;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FeedbackAPI.Services
{
    public class FeedbackAnalysisService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public FeedbackAnalysisService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<AIResponse> AnalyzeAsync(string feedbackText)
        {
            string apiKey = _configuration["OpenRouter:ApiKey"] ?? "";
            string baseUrl = _configuration["OpenRouter:BaseUrl"] ?? "";
            string model = _configuration["OpenRouter:Model"] ?? "";
            string prompt = FeedbackAnalysisPrompt.Build(feedbackText);

            var openRouterRequest = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.2
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, baseUrl)
            {
                Content = JsonContent.Create(openRouterRequest)
            };
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var aiResponse = await _httpClient.SendAsync(requestMessage);
            string responseString = await aiResponse.Content.ReadAsStringAsync();

            return ParseResponse(responseString) ?? CreateFallbackResponse();
        }

        private static AIResponse? ParseResponse(string responseString)
        {
            try
            {
                using var document = JsonDocument.Parse(responseString);
                string text = document.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";

                text = text.Replace("```json", "");
                text = text.Replace("```", "");
                text = text.Trim();

                int start = text.IndexOf("{");
                int end = text.LastIndexOf("}") + 1;

                if (start != -1 && end != -1)
                {
                    text = text.Substring(start, end - start);
                }

                return JsonSerializer.Deserialize<AIResponse>(text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing AI response: {ex.Message}");
                return null;
            }
        }

        private static AIResponse CreateFallbackResponse()
        {
            return new AIResponse
            {
                summary = "Error analyzing feedback",
                sentiment = "NEUTRAL",
                category = "general",
                recommendedAction = "Review manually"
            };
        }
    }
}
