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
                List<object> responseList = new List<object>();

                foreach (var item in feedbackList)
                {
                    responseList.Add(new
                    {
                        feedbackId = item.FeedbackId,
                        customerId = item.CustomerId,
                        customerName = item.CustomerName,
                        customerEmail = item.CustomerEmail,
                        feedbackText = item.FeedbackText,
                        summary = item.Summary,
                        sentiment = item.Sentiment,
                        issueCategory = item.IssueCategory,
                        recommendedAction = item.RecommendedAction,
                        submittedBy = item.CustomerName,
                        createdAt = item.CreatedAt
                    });
                }

                return Ok(responseList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Unable to load feedback records: " + ex.Message);
            }
        }
    }
}
