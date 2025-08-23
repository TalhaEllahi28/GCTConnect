using Microsoft.AspNetCore.Mvc;

using GCTConnect.Services;
using CollegeChatbot.Services;

namespace CollegeChatbot.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly GeminiChatService _chatService;

        public ChatController(GeminiChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] UserQuery query)
        {
            var response = await _chatService.GetResponse(query.Question);
            return Ok(new { answer = response });
        }
    }

    public class UserQuery
    {
        public string Question { get; set; }
    }

}

