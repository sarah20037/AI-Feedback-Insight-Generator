using FeedbackAPI.Models;

namespace FeedbackAPI.Controllers
{
    public partial class FeedbackController
    {
        private static object MakeFeedbackResponse(Feedback feedback)
        {
            return new
            {
                feedbackId = feedback.FeedbackId,
                customerId = feedback.CustomerId,
                customerName = feedback.CustomerName,
                customerEmail = feedback.CustomerEmail,
                feedbackText = feedback.FeedbackText,
                summary = feedback.Summary,
                sentiment = feedback.Sentiment,
                issueCategory = feedback.IssueCategory,
                recommendedAction = feedback.RecommendedAction,
                submittedBy = feedback.CustomerName,
                createdAt = feedback.CreatedAt
            };
        }
    }
}
