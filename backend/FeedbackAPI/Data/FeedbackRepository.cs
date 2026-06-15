using FeedbackAPI.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FeedbackAPI.Data
{
    public partial class FeedbackRepository
    {
        private readonly IConfiguration _configuration;

        public FeedbackRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int SubmitFeedback(FeedbackSubmitRequest request, AIResponse aiResult)
        {
            string? connectionString = _configuration.GetConnectionString("DefaultConnection");

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

            return (int)cmd.ExecuteScalar();
        }

        public List<Feedback> GetAllFeedback()
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
                feedbackList.Add(MapFeedback(reader));
            }

            return feedbackList;
        }

        private static Feedback MapFeedback(SqlDataReader reader)
        {
            return new Feedback
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
            };
        }
    }
}
