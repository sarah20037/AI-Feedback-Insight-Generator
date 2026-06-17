using Microsoft.AspNetCore.Mvc;
using FeedbackAPI.Services;

namespace FeedbackAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public partial class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly PasswordHashingService _passwordHashingService;

        public AuthController(IConfiguration configuration, PasswordHashingService passwordHashingService)
        {
            _configuration = configuration;
            _passwordHashingService = passwordHashingService;
        }
    }
}
