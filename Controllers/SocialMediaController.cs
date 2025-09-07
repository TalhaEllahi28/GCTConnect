using Microsoft.AspNetCore.Mvc;

namespace GCTConnect.Controllers
{
    [Route("social-media")]
    public class SocialMediaController : Controller
    {

        [HttpGet("home")]
        public IActionResult Home()
        {
            return View();
        }
    }
}
