using Microsoft.AspNetCore.Mvc;

using GCTConnect.Services;
using CollegeChatbot.Services;

namespace CollegeChatbot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly OpenAIService _mistralService;

        public ChatbotController(OpenAIService mistralService)
        {
            _mistralService = mistralService;
        }

        [HttpGet("ask")]
        public async Task<IActionResult> Ask([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query cannot be empty.");
            }

            var response = await _mistralService.AskQuestionAsync(query);
            return Ok(new { response });
        }
    }

}

