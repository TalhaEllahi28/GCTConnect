using GCTConnect.Models;
using GCTConnect.Services;
using Microsoft.AspNetCore.Mvc;

namespace GCTConnect.Controllers
{
    [Route("social-media")]
    public class SocialMediaController : Controller
    {
        private readonly IUserService _userService;
        private readonly ISocialMediaService _socialMediaService;
        private readonly ILogger<SocialMediaController> _logger;
        private readonly GctConnectContext _context;

        public SocialMediaController(
            IUserService userService,
            ISocialMediaService socialMediaService,
            ILogger<SocialMediaController> logger,
            GctConnectContext context)
        {
            _userService = userService;
            _socialMediaService = socialMediaService;
            _logger = logger;
            _context = context;
        }

        [HttpGet("home")]
        public async Task<IActionResult> Home()
        {
            var user = _userService.GetCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }

            ViewData["CurrentUserRole"] = user.Role;
            ViewBag.ProfilePic = user.ProfilePic;
            ViewBag.User = "Student";
            // Fixed query - removed the double == true which was causing issues
            var unreadCount = _context.AnnouncementRecipients
                .Count(ar => ar.UserId == user.UserId && ar.IsRead == false);
            ViewBag.UserID = user.UserId;
            ViewBag.UnreadAnnouncements = unreadCount;
            try
            {
                // For demo purposes, using userId = 1. In production, get from JWT/session
                var userId = GetCurrentUserId();
                var profile = await _socialMediaService.GetUserProfileAsync(userId);

                ViewBag.UserProfile = profile;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading social media home");
                return View();
            }
        }

        private int GetCurrentUserId()
        {
            // For demo purposes, returning 1. In production, implement proper authentication
            // return int.Parse(User.FindFirst("UserId")?.Value ?? "1");
            return 1;
        }
    }
}