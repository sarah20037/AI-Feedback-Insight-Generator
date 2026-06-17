using FeedbackAPI.Models;
using System.Text.Json;

namespace FeedbackAPI.Services
{
    public partial class FeedbackAnalysisService
    {
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
