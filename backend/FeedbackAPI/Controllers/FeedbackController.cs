using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using FeedbackAPI.Models;
using System.Data;
using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers;
namespace FeedbackAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public FeedbackController(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        // GET ALL FEEDBACK

        [HttpGet]

        public IActionResult GetAllFeedback()
        {
            try
            {
                string? connectionString = _configuration.GetConnectionString("DefaultConnection");

                List<Feedback> feedbackList = new List<Feedback>();

                using var con = new SqlConnection(connectionString);
                con.Open();

                using var cmd = new SqlCommand("sp_GetFeedbackWithCustomer", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 5;

                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    feedbackList.Add(new Feedback
                    {
                        FeedbackId = Convert.ToInt32(reader["FeedbackId"]),
                        CustomerId = Convert.ToInt32(reader["CustomerId"]),
                        CustomerName = reader["CustomerName"].ToString() ?? "",
                        CustomerEmail = reader["CustomerEmail"].ToString() ?? "",
                        FeedbackText = reader["FeedbackText"].ToString() ?? "",
                        Summary = reader["Summary"].ToString() ?? "",
                        Sentiment = reader["Sentiment"].ToString() ?? "",
                        IssueCategory = reader["IssueCategory"].ToString() ?? "",
                        RecommendedAction = reader["RecommendedAction"].ToString() ?? "",
                        CreatedAt = reader["SubmittedAt"] == DBNull.Value 
                            ? DateTime.Now 
                            : Convert.ToDateTime(reader["SubmittedAt"])
                    });
                }

                return Ok(feedbackList.Select(f => new {
                    feedbackId = f.FeedbackId,
                    customerId = f.CustomerId,
                    customerName = f.CustomerName,
                    customerEmail = f.CustomerEmail,
                    feedbackText = f.FeedbackText,
                    summary = f.Summary,
                    sentiment = f.Sentiment,
                    issueCategory = f.IssueCategory,
                    recommendedAction = f.RecommendedAction,
                    submittedBy = f.CustomerName,
                    createdAt = f.CreatedAt
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unable to load feedback records: {ex.Message}");
            }
        }


        // SUBMIT FEEDBACK

        [HttpPost("submit")]

        public async Task<IActionResult> SubmitFeedback(
            FeedbackSubmitRequest request)
        {
            if (request.CustomerId <= 0)
            {
                return BadRequest("CustomerId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.FeedbackText))
            {
                return BadRequest("Feedback text is required.");
            }

            string apiKey = _configuration["OpenRouter:ApiKey"] ?? "";
            string baseUrl = _configuration["OpenRouter:BaseUrl"] ?? "";
            string model = _configuration["OpenRouter:Model"] ?? "";

            string prompt = $@"
You are an API.

You MUST return ONLY valid JSON.

No explanation.
No markdown.
No headings.
No extra text.

Analyze this customer feedback:

""{request.FeedbackText}""

Return EXACTLY this format:

{{
  ""summary"": ""short summary"",
  ""sentiment"": ""POSITIVE or NEGATIVE or NEUTRAL"",
  ""category"": ""issue category"",
  ""recommendedAction"": ""recommended fix""
}}

ONLY RETURN JSON.
";

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

            AIResponse? aiResult = null;
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

                aiResult = JsonSerializer.Deserialize<AIResponse>(text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing AI response: {ex.Message}");
            }

            if (aiResult == null)
            {
                aiResult = new AIResponse 
                {
                    summary = "Error analyzing feedback",
                    sentiment = "NEUTRAL",
                    category = "general",
                    recommendedAction = "Review manually"
                };               
            }

            string? connectionString =
                _configuration.GetConnectionString(
                    "DefaultConnection");

            int feedbackId;

            try
            {
                using var con = new SqlConnection(connectionString);
                con.Open();

                using var cmd = new SqlCommand("sp_SubmitFeedback", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 5;

                cmd.Parameters.AddWithValue("@CustomerId", request.CustomerId);
                cmd.Parameters.AddWithValue("@FeedbackText", request.FeedbackText);
                cmd.Parameters.AddWithValue("@Summary", aiResult.summary);
                cmd.Parameters.AddWithValue("@Sentiment", aiResult.sentiment);
                cmd.Parameters.AddWithValue("@IssueCategory", aiResult.category);
                cmd.Parameters.AddWithValue("@RecommendedAction", aiResult.recommendedAction);

                feedbackId = (int)cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Feedback could not be saved: {ex.Message}");
            }

            return Ok(new
            {
                message = "Feedback Submitted Successfully",
                feedback = new {
                    feedbackId = feedbackId,
                    customerId = request.CustomerId,
                    customerName = "",
                    customerEmail = "",
                    feedbackText = request.FeedbackText,
                    summary = aiResult.summary,
                    sentiment = aiResult.sentiment,
                    issueCategory = aiResult.category,
                    recommendedAction = aiResult.recommendedAction,
                    submittedBy = "",
                    createdAt = DateTime.Now
                },
                aiAnalysis = aiResult
            });
        }
    }
}
