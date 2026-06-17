using FeedbackAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackAPI.Controllers
{
    public partial class FeedbackController
    {
        [HttpGet("customer/{customerId:int}")]
        public IActionResult GetCustomerFeedback(int customerId)
        {
            try
            {
                if (customerId <= 0)
                {
                    return BadRequest("CustomerId is required.");
                }

                List<Feedback> feedbackList = _feedbackRepository.GetAllFeedback()
                    .Where(feedback => feedback.CustomerId == customerId)
                    .ToList();

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
