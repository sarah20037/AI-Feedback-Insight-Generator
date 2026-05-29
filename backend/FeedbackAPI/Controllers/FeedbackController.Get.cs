using FeedbackAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackAPI.Controllers
{
    public partial class FeedbackController
    {
        [HttpGet]
        public IActionResult GetAllFeedback()
        {
            try
            {
                List<Feedback> feedbackList = _feedbackRepository.GetAllFeedback();

                return Ok(feedbackList.Select(f => new
                {
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
    }
}
