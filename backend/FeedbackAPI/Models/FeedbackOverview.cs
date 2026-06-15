namespace FeedbackAPI.Models
{
    public class FeedbackOverview
    {
        public List<Feedback> Items { get; set; } = new List<Feedback>();

        public List<Feedback> LatestNegativeItems { get; set; } = new List<Feedback>();

        public int TotalCount { get; set; }

        public int PositiveCount { get; set; }

        public int NegativeCount { get; set; }

        public int NeutralCount { get; set; }
    }
}
