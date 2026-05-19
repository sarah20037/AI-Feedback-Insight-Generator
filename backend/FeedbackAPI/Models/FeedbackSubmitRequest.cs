namespace FeedbackAPI.Models
{
    public class FeedbackSubmitRequest
    {
        public int CustomerId { get; set; }

        public string FeedbackText { get; set; } = string.Empty;
    }
}
