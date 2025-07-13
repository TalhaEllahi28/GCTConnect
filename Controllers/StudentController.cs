using GCTConnect.Models;
using GCTConnect.Services;
using Microsoft.AspNetCore.Mvc;

namespace GCTConnect.Controllers
{
    public class StudentController : Controller
    {


        private readonly IUserService _userService;
        private readonly ILogger<AdminController> _logger;
        private readonly GctConnectContext _context;
        public StudentController(ILogger<AdminController> logger, GctConnectContext context, IUserService userService)
        {
            _logger = logger;
            _context = context;
            _userService = userService;
        }
        public IActionResult Dashboard()
        {
            var user=_userService.GetCurrentUser();
            ViewData["CurrentUserRole"]= user.Role;
            ViewBag.ProfilePic = user.ProfilePic;


            return View();
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
