using FeedbackAPI.Models;

namespace FeedbackAPI.Data
{
    public partial class FeedbackRepository
    {
        public FeedbackOverview GetFeedbackOverview()
        {
            List<Feedback> feedbacks = GetAllFeedback()
                .OrderByDescending(feedback => feedback.CreatedAt)
                .ToList();

            return new FeedbackOverview
            {
                Items = feedbacks.Take(10).ToList(),
                LatestNegativeItems = GetTopNegativeFeedbacks(feedbacks),
                TotalCount = feedbacks.Count,
                PositiveCount = CountSentiment(feedbacks, "POSITIVE"),
                NegativeCount = CountSentiment(feedbacks, "NEGATIVE"),
                NeutralCount = CountSentiment(feedbacks, "NEUTRAL")
            };
        }

        public FeedbackPageResult GetFeedbackPage(int page, int pageSize)
        {
            int safePage = Math.Max(page, 1);
            int safePageSize = Math.Clamp(pageSize, 1, 50);
            List<Feedback> feedbacks = GetAllFeedback()
                .OrderByDescending(feedback => feedback.CreatedAt)
                .ToList();

            return new FeedbackPageResult
            {
                Items = feedbacks
                    .Skip((safePage - 1) * safePageSize)
                    .Take(safePageSize)
                    .ToList(),
                Page = safePage,
                PageSize = safePageSize,
                TotalCount = feedbacks.Count,
                PositiveCount = CountSentiment(feedbacks, "POSITIVE"),
                NegativeCount = CountSentiment(feedbacks, "NEGATIVE"),
                NeutralCount = CountSentiment(feedbacks, "NEUTRAL")
            };
        }

        private static int CountSentiment(IEnumerable<Feedback> feedbacks, string sentiment)
        {
            return feedbacks.Count(feedback => feedback.Sentiment == sentiment);
        }

        private static List<Feedback> GetTopNegativeFeedbacks(List<Feedback> feedbacks)
        {
            List<Feedback> negativeFeedbacks = feedbacks
                .Where(feedback => feedback.Sentiment == "NEGATIVE")
                .ToList();

            Dictionary<string, int> repeatCounts = negativeFeedbacks
                .GroupBy(feedback => string.IsNullOrWhiteSpace(feedback.IssueCategory)
                    ? "General negative feedback"
                    : feedback.IssueCategory)
                .ToDictionary(group => group.Key, group => group.Count());

            return negativeFeedbacks
                .OrderByDescending(feedback => repeatCounts[string.IsNullOrWhiteSpace(feedback.IssueCategory)
                    ? "General negative feedback"
                    : feedback.IssueCategory])
                .ThenByDescending(feedback => feedback.CreatedAt)
                .Take(5)
                .ToList();
        }
    }
}
