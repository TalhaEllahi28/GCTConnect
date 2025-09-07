// Controllers/ChatController.cs
using Microsoft.AspNetCore.Mvc;
using GCTConnect.Services;
using Org.BouncyCastle.Asn1.Crmf;
using GCTConnect.Models;


[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IGeminiService _geminiService;
    private readonly IUserService _userService;
    private readonly GctConnectContext _context;  
    public ChatController(IGeminiService geminiService, IUserService userService, GctConnectContext context)
    {
        _geminiService = geminiService;
        _userService = userService;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ChatbotQuery request)
    {
        if (string.IsNullOrEmpty(request.QueryText))
        {
            return BadRequest("QueryText is required");
        }

        try
        {
            var response = await _geminiService.GetResponseAsync(request.QueryText);
            if (response == null) {
                response = "Something went wrong in server side";
            }
            var user = _userService.GetCurrentUser();   
            if (user == null)
            {
                return Unauthorized();
            }
            var query = new ChatbotQuery
            {
                UserId = user.UserId,
                QueryText = request.QueryText,
                Response = response,
                Timestamp = DateTime.UtcNow
            };

            // Save to database
            await _context.ChatbotQueries.AddAsync(query);
            await _context.SaveChangesAsync();
            
            return Ok(new { response });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An error occurred while processing your request" });
        }
    }
}