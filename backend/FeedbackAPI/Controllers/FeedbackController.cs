using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using FeedbackAPI.Models;
using System.Data;
namespace FeedbackAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public FeedbackController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET ALL FEEDBACK

        [HttpGet]

        public IActionResult GetAllFeedback()
        {
            try
            {
                string? connectionString =
                    _configuration.GetConnectionString("DefaultConnection");

                List<Feedback> feedbackList = new List<Feedback>();

                using (SqlConnection con =
                    new SqlConnection(connectionString))
                {
                    con.Open();

                    SqlCommand cmd =
                        new SqlCommand("sp_GetFeedbackWithCustomer", con);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 5;

                    SqlDataReader reader =
                        cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        feedbackList.Add(new Feedback
                        {
                            FeedbackId =
                                Convert.ToInt32(reader["FeedbackId"]),

                            CustomerId =
                                Convert.ToInt32(reader["CustomerId"]),

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
                                reader["SubmittedAt"] == DBNull.Value
                                    ? DateTime.Now
                                    : Convert.ToDateTime(reader["SubmittedAt"])
                        });
                    }
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

        public IActionResult SubmitFeedback(
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

            string? connectionString =
                _configuration.GetConnectionString(
                    "DefaultConnection");

            AIResponse aiResult = AnalyzeFeedbackFast(request.FeedbackText);
            int feedbackId;

            try
            {
                using (SqlConnection con =
                    new SqlConnection(connectionString))
                {
                    con.Open();

                    SqlCommand cmd =
                        new SqlCommand("sp_SubmitFeedback", con);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 5;

                    cmd.Parameters.AddWithValue(
                        "@CustomerId",
                        request.CustomerId);

                    cmd.Parameters.AddWithValue(
                        "@FeedbackText",
                        request.FeedbackText);

                    cmd.Parameters.AddWithValue(
                        "@Summary",
                        aiResult.summary);

                    cmd.Parameters.AddWithValue(
                        "@Sentiment",
                        aiResult.sentiment);

                    cmd.Parameters.AddWithValue(
                        "@IssueCategory",
                        aiResult.category);

                    cmd.Parameters.AddWithValue(
                        "@RecommendedAction",
                        aiResult.recommendedAction);

                    feedbackId =
                        (int)cmd.ExecuteScalar();
                }
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

        private static AIResponse AnalyzeFeedbackFast(string feedbackText)
        {
            string text = feedbackText.ToLowerInvariant();

            string[] negativeWords = { "bad", "slow", "difficult", "crash", "error", "issue", "problem", "poor", "delay", "not working", "hard" };
            string[] positiveWords = { "good", "great", "excellent", "easy", "fast", "nice", "helpful", "love", "satisfied" };

            bool isNegative = negativeWords.Any(text.Contains);
            bool isPositive = positiveWords.Any(text.Contains);

            string sentiment = isNegative && !isPositive
                ? "NEGATIVE"
                : isPositive && !isNegative
                    ? "POSITIVE"
                    : "NEUTRAL";

            string category = text.Contains("slow") || text.Contains("delay") || text.Contains("fast")
                ? "performance"
                : text.Contains("crash") || text.Contains("error") || text.Contains("not working")
                    ? "issue"
                    : text.Contains("difficult") || text.Contains("hard") || text.Contains("easy")
                        ? "usability"
                        : "general";

            string recommendedAction = sentiment == "NEGATIVE"
                ? category == "performance"
                    ? "Optimize response time"
                    : category == "issue"
                        ? "Investigate and fix the reported issue"
                        : "Review the feedback and follow up with the customer"
                : "No action required";

            string summary = sentiment == "POSITIVE"
                ? "Customer shared positive feedback"
                : sentiment == "NEGATIVE"
                    ? "Customer reported an issue"
                    : "Customer shared neutral feedback";

            return new AIResponse
            {
                summary = summary,
                sentiment = sentiment,
                category = category,
                recommendedAction = recommendedAction
            };
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
