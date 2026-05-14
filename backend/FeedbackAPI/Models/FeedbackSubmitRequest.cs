namespace FeedbackAPI.Models
{
    public class FeedbackSubmitRequest
    {
        public string CustomerName { get; set; } = string.Empty;

        public string CustomerEmail { get; set; } = string.Empty;

        public string FeedbackText { get; set; } = string.Empty;
    }
}
