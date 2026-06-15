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

                return Ok(feedbackList.Select(MakeFeedbackResponse));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unable to load feedback records: {ex.Message}");
            }
        }

        [HttpGet("overview")]
        public IActionResult GetFeedbackOverview()
        {
            try
            {
                FeedbackOverview overview = _feedbackRepository.GetFeedbackOverview();

                return Ok(new
                {
                    items = overview.Items.Select(MakeFeedbackResponse),
                    latestNegativeItems = overview.LatestNegativeItems.Select(MakeFeedbackResponse),
                    totalCount = overview.TotalCount,
                    positiveCount = overview.PositiveCount,
                    negativeCount = overview.NegativeCount,
                    neutralCount = overview.NeutralCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unable to load feedback overview: {ex.Message}");
            }
        }

        [HttpGet("page")]
        public IActionResult GetFeedbackPage([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                FeedbackPageResult result = _feedbackRepository.GetFeedbackPage(page, pageSize);

                return Ok(new
                {
                    items = result.Items.Select(MakeFeedbackResponse),
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
                return StatusCode(500, $"Unable to load feedback page: {ex.Message}");
            }
        }

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

                return Ok(feedbackList.Select(MakeFeedbackResponse));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Unable to load feedback records: {ex.Message}");
            }
        }
    }
}
