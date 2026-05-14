using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using FeedbackAPI.Models;
using System.Net.Http.Json;

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

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitFeedback(FeedbackSubmitRequest request)
        {
            try
            {
                // PYTHON AI API URL
                string aiUrl = "http://127.0.0.1:8000/analyze_feedback";

                // SEND FEEDBACK TO AI
                var aiRequest = new
                {
                    text = request.FeedbackText
                };

                var aiResponse =
                    await _httpClient.PostAsJsonAsync(aiUrl, aiRequest);

                if (!aiResponse.IsSuccessStatusCode)
                {
                    return StatusCode(500, "AI Service Failed");
                }

                // GET AI RESULT
                var aiResult =
                    await aiResponse.Content.ReadFromJsonAsync<AIResponse>();

                // DATABASE CONNECTION
                string connectionString =
                    _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"
                    INSERT INTO Feedback
                    (
                        CustomerId,
                        FeedbackText,
                        Summary,
                        Sentiment,
                        IssueCategory,
                        RecommendedAction
                    )

                    VALUES

                    (
                        @CustomerId,
                        @FeedbackText,
                        @Summary,
                        @Sentiment,
                        @IssueCategory,
                        @RecommendedAction
                    )
                    ";

                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@CustomerId", request.CustomerId);

                    cmd.Parameters.AddWithValue("@FeedbackText", request.FeedbackText);

                    cmd.Parameters.AddWithValue("@Summary", aiResult.summary);

                    cmd.Parameters.AddWithValue("@Sentiment", aiResult.sentiment);

                    cmd.Parameters.AddWithValue("@IssueCategory", aiResult.category);

                    cmd.Parameters.AddWithValue("@RecommendedAction", aiResult.recommendedAction);

                    cmd.ExecuteNonQuery();
                }

                return Ok(new
                {
                    message = "Feedback Submitted Successfully",
                    aiAnalysis = aiResult
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }

    // AI RESPONSE MODEL
    public class AIResponse
    {
        public string summary { get; set; }

        public string sentiment { get; set; }

        public string category { get; set; }

        public string recommendedAction { get; set; }
    }
}