using FeedbackAPI.Services;
using Xunit;

namespace FeedbackAPI.Tests.Services
{
    public class PasswordHashingServiceTests
    {
        private readonly PasswordHashingService _service = new();

        [Fact]
        public void HashPasswordCreatesVerifiablePbkdf2Hash()
        {
            string hash = _service.HashPassword("CorrectHorseBatteryStaple");

            Assert.StartsWith("PBKDF2$100000$", hash);
            Assert.NotEqual("CorrectHorseBatteryStaple", hash);
            Assert.True(_service.VerifyPassword("CorrectHorseBatteryStaple", hash));
        }

        [Fact]
        public void VerifyPasswordRejectsWrongPassword()
        {
            string hash = _service.HashPassword("CorrectHorseBatteryStaple");

            Assert.False(_service.VerifyPassword("wrong-password", hash));
        }

        [Fact]
        public void VerifyPasswordSupportsLegacyPlainTextPasswords()
        {
            Assert.True(_service.VerifyPassword("legacy-password", "legacy-password"));
            Assert.False(_service.VerifyPassword("wrong-password", "legacy-password"));
        }

        [Fact]
        public void VerifyPasswordRejectsMalformedHash()
        {
            Assert.False(_service.VerifyPassword("password", "PBKDF2$100000$not-base64$also-not-base64"));
        }
    }
}
