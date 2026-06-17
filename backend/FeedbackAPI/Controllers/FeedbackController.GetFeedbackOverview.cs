using FeedbackAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackAPI.Controllers
{
    public partial class FeedbackController
    {
        [HttpGet("overview")]
        public IActionResult GetFeedbackOverview()
        {
            try
            {
                FeedbackOverview overview = _feedbackRepository.GetFeedbackOverview();

                List<object> itemsList = new List<object>();
                foreach (var item in overview.Items)
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

                List<object> negativeList = new List<object>();
                foreach (var item in overview.LatestNegativeItems)
                {
                    negativeList.Add(new
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
                    latestNegativeItems = negativeList,
                    totalCount = overview.TotalCount,
                    positiveCount = overview.PositiveCount,
                    negativeCount = overview.NegativeCount,
                    neutralCount = overview.NeutralCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Unable to load feedback overview: " + ex.Message);
            }
        }
    }
}
