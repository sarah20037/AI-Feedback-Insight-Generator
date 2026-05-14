namespace FeedbackAPI.Models
{
    public class Feedback
    {
        public int FeedbackId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public string CustomerEmail { get; set; } = string.Empty;

        public string FeedbackText { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public string Sentiment { get; set; } = string.Empty;

        public string IssueCategory { get; set; } = string.Empty;

        public string RecommendedAction { get; set; } = string.Empty;

        public string SubmittedBy { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
