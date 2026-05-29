using FeedbackAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace FeedbackAPI.Controllers
{
    public partial class FeedbackController
    {
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitFeedback(FeedbackSubmitRequest request)
        {
            if (request.CustomerId <= 0)
            {
                return BadRequest("CustomerId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.FeedbackText))
            {
                return BadRequest("Feedback text is required.");
            }

            AIResponse aiResult = await _analysisService.AnalyzeAsync(request.FeedbackText);
            int feedbackId;

            try
            {
                feedbackId = _feedbackRepository.SubmitFeedback(request, aiResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Feedback could not be saved: {ex.Message}");
            }

            return Ok(new
            {
                message = "Feedback Submitted Successfully",
                feedback = new
                {
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
    }
}
