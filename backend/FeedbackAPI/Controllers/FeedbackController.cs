using Microsoft.AspNetCore.Mvc;
using FeedbackAPI.Data;
using FeedbackAPI.Services;

namespace FeedbackAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public partial class FeedbackController : ControllerBase
    {
        private readonly FeedbackAnalysisService _analysisService;
        private readonly FeedbackRepository _feedbackRepository;

        public FeedbackController(FeedbackAnalysisService analysisService, FeedbackRepository feedbackRepository)
        {
            _analysisService = analysisService;
            _feedbackRepository = feedbackRepository;
        }
    }
}
