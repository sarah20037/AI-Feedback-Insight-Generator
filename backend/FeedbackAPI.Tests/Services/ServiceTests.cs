using FeedbackAPI.Services;
using Xunit;

namespace FeedbackAPI.Tests.Services
{
    public class ServiceTests
    {
        [Fact]
        public void PasswordHashCanBeVerified()
        {
            var service = new PasswordHashingService();

            string hash = service.HashPassword("secret123");

            Assert.NotEqual("secret123", hash);
            Assert.True(service.VerifyPassword("secret123", hash));
            Assert.False(service.VerifyPassword("wrong", hash));
        }

        [Fact]
        public void OldPlainTextPasswordsStillWork()
        {
            var service = new PasswordHashingService();

            Assert.True(service.VerifyPassword("oldpass", "oldpass"));
            Assert.False(service.VerifyPassword("wrong", "oldpass"));
        }

        [Fact]
        public void AiResponseParserReadsJsonContent()
        {
            string response = """
            {
              "choices": [
                {
                  "message": {
                    "content": "{\"summary\":\"Slow support\",\"sentiment\":\"NEGATIVE\",\"category\":\"support\",\"recommendedAction\":\"Call customer\"}"
                  }
                }
              ]
            }
            """;

            var result = FeedbackAnalysisService.ParseResponse(response);

            Assert.NotNull(result);
            Assert.Equal("Slow support", result.summary);
            Assert.Equal("NEGATIVE", result.sentiment);
            Assert.Equal("support", result.category);
        }
    }
}
