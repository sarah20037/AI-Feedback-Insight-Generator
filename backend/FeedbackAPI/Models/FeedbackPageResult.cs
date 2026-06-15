namespace FeedbackAPI.Models
{
    public class FeedbackPageResult
    {
        public List<Feedback> Items { get; set; } = new List<Feedback>();

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int PositiveCount { get; set; }

        public int NegativeCount { get; set; }

        public int NeutralCount { get; set; }
    }
}
