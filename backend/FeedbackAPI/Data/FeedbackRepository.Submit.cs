using FeedbackAPI.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace FeedbackAPI.Data
{
    public partial class FeedbackRepository
    {
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
    }
}
