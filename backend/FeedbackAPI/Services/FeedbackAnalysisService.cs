using FeedbackAPI.Models;
using FeedbackAPI.Prompts;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FeedbackAPI.Services
{
    public partial class FeedbackAnalysisService
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
    }
}
