using GCTConnect.Models;
using GCTConnect.Services;
using Microsoft.AspNetCore.Mvc;

namespace GCTConnect.Controllers
{
    public class HodController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<AdminController> _logger;
        private readonly GctConnectContext _context;
        public HodController(ILogger<AdminController> logger, GctConnectContext context, IUserService userService)
        {
            _logger = logger;
            _context = context;
            _userService = userService;
        }
        public IActionResult Dashboard()
        {
            var user = _userService.GetCurrentUser();
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }

            ViewData["CurrentUserRole"] = user.Role;
            ViewBag.ProfilePic = user.ProfilePic;
            ViewBag.User = "Hod";
            // Fixed query - removed the double == true which was causing issues
            var unreadCount = _context.AnnouncementRecipients
                .Count(ar => ar.UserId == user.UserId && ar.IsRead == false);
            ViewBag.UserID = user.UserId;
            ViewBag.UnreadAnnouncements = unreadCount;
            return View();
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
