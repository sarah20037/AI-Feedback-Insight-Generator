using FeedbackAPI.Controllers;
using FeedbackAPI.Models;
using FeedbackAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace FeedbackAPI.Tests.Controllers
{
    public class ApiControllerTests
    {
        [Fact]
        public void RegisterReturnsBadRequestWhenRequiredFieldsAreMissing()
        {
            var controller = new AuthController(new ConfigurationBuilder().Build(), new PasswordHashingService());

            var result = controller.Register(new Customer());

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void AdminLoginReturnsOkForConfiguredAdmin()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AdminAccount:Email"] = "admin",
                    ["AdminAccount:Password"] = "pass123"
                })
                .Build();
            var controller = new AuthController(config, new PasswordHashingService());

            var result = controller.AdminLogin(new LoginRequest { Username = "admin", PasswordHash = "pass123" });

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task SubmitFeedbackReturnsBadRequestForInvalidInput()
        {
            var controller = new FeedbackController(null!, null!);

            var result = await controller.SubmitFeedback(new FeedbackSubmitRequest());

            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
