namespace FeedbackAPI.Models
{
    public class Feedback
    {
        public int FeedbackId { get; set; }

        public int CustomerId { get; set; }

        public string FeedbackText { get; set; }

        public string Summary { get; set; }

        public string Sentiment { get; set; }

        public string Emotion { get; set; }
    }
}