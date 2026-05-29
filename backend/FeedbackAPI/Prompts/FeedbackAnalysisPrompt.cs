namespace FeedbackAPI.Prompts
{
    public static class FeedbackAnalysisPrompt
    {
        private const string PromptFileName = "feedback-analysis-prompt.txt";
        private const string FeedbackPlaceholder = "{{feedbackText}}";

        public static string Build(string feedbackText)
        {
            string promptPath = Path.Combine(AppContext.BaseDirectory, "Prompts", PromptFileName);
            string template = File.ReadAllText(promptPath);

            return template.Replace(FeedbackPlaceholder, feedbackText);
        }
    }
}
