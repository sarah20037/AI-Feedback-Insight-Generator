using FeedbackAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackAPI.Controllers
{
    public partial class FeedbackController
    {
        [HttpGet("page")]
        public IActionResult GetFeedbackPage([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                FeedbackPageResult result = _feedbackRepository.GetFeedbackPage(page, pageSize);

                List<object> itemsList = new List<object>();
                foreach (var item in result.Items)
                {
                    itemsList.Add(new
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

                return Ok(new
                {
                    items = itemsList,
                    page = result.Page,
                    pageSize = result.PageSize,
                    totalCount = result.TotalCount,
                    positiveCount = result.PositiveCount,
                    negativeCount = result.NegativeCount,
                    neutralCount = result.NeutralCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Unable to load feedback page: " + ex.Message);
            }
        }
    }
}
