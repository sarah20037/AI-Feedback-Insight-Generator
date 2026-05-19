using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using FeedbackAPI.Models;
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
            string? connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            List<Feedback> feedbackList = new List<Feedback>();

            using (SqlConnection con =
                new SqlConnection(connectionString))
            {
                con.Open();

                string query = @"
                SELECT * FROM Feedback
                ORDER BY FeedbackId DESC
                ";

                SqlCommand cmd =
                    new SqlCommand(query, con);

                SqlDataReader reader =
                    cmd.ExecuteReader();

                while (reader.Read())
                {
                    feedbackList.Add(new Feedback
                    {
                        FeedbackId =
                            Convert.ToInt32(reader["FeedbackId"]),

                        CustomerName =
                            reader["CustomerName"].ToString()
                            ?? "",

                        CustomerEmail =
                            reader["CustomerEmail"].ToString()
                            ?? "",

                        FeedbackText =
                            reader["FeedbackText"].ToString()
                            ?? "",

                        Summary =
                            reader["Summary"].ToString()
                            ?? "",

                        Sentiment =
                            reader["Sentiment"].ToString()
                            ?? "",

                        IssueCategory =
                            reader["IssueCategory"].ToString()
                            ?? "",

                        RecommendedAction =
                            reader["RecommendedAction"].ToString()
                            ?? "",

                        CreatedAt =
                            DateTime.Now
                    });
                }
            }

            return Ok(feedbackList);
        }


        // SUBMIT FEEDBACK

        [HttpPost("submit")]

        public async Task<IActionResult> SubmitFeedback(
            FeedbackSubmitRequest request)
        {
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


            string? connectionString =
                _configuration.GetConnectionString(
                    "DefaultConnection");


            int feedbackId;

            using (SqlConnection con =
                new SqlConnection(connectionString))
            {
                con.Open();

                string query = @"

                INSERT INTO Feedback
                (
                    CustomerName,
                    CustomerEmail,
                    FeedbackText,
                    Summary,
                    Sentiment,
                    IssueCategory,
                    RecommendedAction
                )

                OUTPUT INSERTED.FeedbackId

                VALUES
                (
                    @CustomerName,
                    @CustomerEmail,
                    @FeedbackText,
                    @Summary,
                    @Sentiment,
                    @IssueCategory,
                    @RecommendedAction
                )
                ";

                SqlCommand cmd =
                    new SqlCommand(query, con);

                cmd.Parameters.AddWithValue(
                    "@CustomerName",
                    request.CustomerName);

                cmd.Parameters.AddWithValue(
                    "@CustomerEmail",
                    request.CustomerEmail);

                cmd.Parameters.AddWithValue(
                    "@FeedbackText",
                    request.FeedbackText);

                cmd.Parameters.AddWithValue(
                    "@Summary",
                    aiResult?.summary ?? "");

                cmd.Parameters.AddWithValue(
                    "@Sentiment",
                    aiResult?.sentiment ?? "");

                cmd.Parameters.AddWithValue(
                    "@IssueCategory",
                    aiResult?.category ?? "");

                cmd.Parameters.AddWithValue(
                    "@RecommendedAction",
                    aiResult?.recommendedAction ?? "");


                feedbackId =
                    (int)cmd.ExecuteScalar();
            }

            return Ok(new
            {
                message = "Feedback Submitted Successfully",

                feedbackId
            });
        }
    }


    // MODEL FOR AI RESPONSE

    public class AIResponse
    {
        public string summary { get; set; } = "";

        public string sentiment { get; set; } = "";

        public string category { get; set; } = "";

        public string recommendedAction { get; set; } = "";
    }
}