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

        [HttpGet]
        public IActionResult GetAllFeedback()
        {
            try
            {
                string? connectionString =
                    _configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return StatusCode(500, "Database connection is not configured.");
                }

                List<Feedback> feedbackList = new List<Feedback>();

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"
                    SELECT
                        f.FeedbackId,
                        ISNULL(f.CustomerName, 'Customer') AS CustomerName,
                        ISNULL(f.CustomerEmail, '') AS CustomerEmail,
                        f.FeedbackText,
                        f.Summary,
                        f.Sentiment,
                        f.IssueCategory,
                        f.RecommendedAction,
                        GETDATE() AS CreatedAt
                    FROM Feedback f
                    ORDER BY f.FeedbackId DESC
                    ";

                    SqlCommand cmd = new SqlCommand(query, con);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        feedbackList.Add(new Feedback
                        {
                            FeedbackId = Convert.ToInt32(reader["FeedbackId"]),
                            CustomerName = reader["CustomerName"].ToString() ?? string.Empty,
                            CustomerEmail = reader["CustomerEmail"].ToString() ?? string.Empty,
                            FeedbackText = reader["FeedbackText"].ToString() ?? string.Empty,
                            Summary = reader["Summary"].ToString() ?? string.Empty,
                            Sentiment = reader["Sentiment"].ToString() ?? string.Empty,
                            IssueCategory = reader["IssueCategory"].ToString() ?? string.Empty,
                            RecommendedAction = reader["RecommendedAction"].ToString() ?? string.Empty,
                            SubmittedBy = reader["CustomerName"].ToString() ?? string.Empty,
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                        });
                    }
                }

                return Ok(feedbackList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitFeedback(FeedbackSubmitRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.FeedbackText))
                {
                    return BadRequest("Feedback text is required.");
                }

                if (string.IsNullOrWhiteSpace(request.CustomerName) ||
                    string.IsNullOrWhiteSpace(request.CustomerEmail))
                {
                    return BadRequest("Customer name and email are required.");
                }

                string? aiUrl = _configuration["AiService:Url"];

                if (string.IsNullOrWhiteSpace(aiUrl))
                {
                    return StatusCode(500, "AI service URL is not configured.");
                }

                AIResponse aiResult = await GetAnalysisWithFallback(aiUrl, request.FeedbackText);

                string? connectionString =
                    _configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return StatusCode(500, "Database connection is not configured.");
                }

                int feedbackId;

                using (SqlConnection con = new SqlConnection(connectionString))
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

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@CustomerName", request.CustomerName);
                    cmd.Parameters.AddWithValue("@CustomerEmail", request.CustomerEmail);
                    cmd.Parameters.AddWithValue("@FeedbackText", request.FeedbackText);
                    cmd.Parameters.AddWithValue("@Summary", aiResult.summary);
                    cmd.Parameters.AddWithValue("@Sentiment", aiResult.sentiment);
                    cmd.Parameters.AddWithValue("@IssueCategory", aiResult.category);
                    cmd.Parameters.AddWithValue("@RecommendedAction", aiResult.recommendedAction);

                    feedbackId = (int)cmd.ExecuteScalar();
                }

                return Ok(new
                {
                    message = "Feedback Submitted Successfully",
                    feedback = new Feedback
                    {
                        FeedbackId = feedbackId,
                        CustomerName = request.CustomerName,
                        CustomerEmail = request.CustomerEmail,
                        FeedbackText = request.FeedbackText,
                        Summary = aiResult.summary,
                        Sentiment = aiResult.sentiment,
                        IssueCategory = aiResult.category,
                        RecommendedAction = aiResult.recommendedAction,
                        SubmittedBy = request.CustomerName,
                        CreatedAt = DateTime.Now
                    },
                    aiAnalysis = aiResult
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        private async Task<AIResponse> GetAnalysisWithFallback(string aiUrl, string feedbackText)
        {
            try
            {
                using CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromSeconds(8));

                var aiRequest = new
                {
                    text = feedbackText
                };

                var aiResponse =
                    await _httpClient.PostAsJsonAsync(aiUrl, aiRequest, timeout.Token);

                if (aiResponse.IsSuccessStatusCode)
                {
                    AIResponse? aiResult =
                        await aiResponse.Content.ReadFromJsonAsync<AIResponse>(cancellationToken: timeout.Token);

                    if (aiResult != null &&
                        !string.IsNullOrWhiteSpace(aiResult.sentiment) &&
                        !string.IsNullOrWhiteSpace(aiResult.summary))
                    {
                        return aiResult;
                    }
                }
            }
            catch
            {
                // If AI is slow or unavailable, save a basic analysis so admin can still review feedback.
            }

            return BuildSimpleAnalysis(feedbackText);
        }

        private AIResponse BuildSimpleAnalysis(string feedbackText)
        {
            string lowerText = feedbackText.ToLower();

            string sentiment = "NEUTRAL";
            string category = "General feedback";
            string action = "Review the feedback manually and assign it to the correct team member.";

            if (lowerText.Contains("difficult") ||
                lowerText.Contains("slow") ||
                lowerText.Contains("bad") ||
                lowerText.Contains("problem") ||
                lowerText.Contains("confusing") ||
                lowerText.Contains("issue"))
            {
                sentiment = "NEGATIVE";
                category = "Usability or service issue";
                action = "Check the reported problem, simplify the process, and follow up with affected users.";
            }
            else if (lowerText.Contains("good") ||
                     lowerText.Contains("great") ||
                     lowerText.Contains("easy") ||
                     lowerText.Contains("helpful") ||
                     lowerText.Contains("excellent"))
            {
                sentiment = "POSITIVE";
                category = "Positive feedback";
                action = "Share this feedback with the team and continue the current approach.";
            }

            return new AIResponse
            {
                summary = feedbackText.Length > 90 ? feedbackText.Substring(0, 90) + "..." : feedbackText,
                sentiment = sentiment,
                category = category,
                recommendedAction = action
            };
        }
    }

    public class AIResponse
    {
        public string summary { get; set; } = string.Empty;

        public string sentiment { get; set; } = string.Empty;

        public string category { get; set; } = string.Empty;

        public string recommendedAction { get; set; } = string.Empty;
    }
}
