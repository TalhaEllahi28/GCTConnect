using GCTConnect.Models;
using GCTConnect.Services;
using Microsoft.AspNetCore.Mvc;

namespace GCTConnect.Controllers
{
    public class PrincipalController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<AdminController> _logger;
        private readonly GctConnectContext _context;
        public PrincipalController(ILogger<AdminController> logger, GctConnectContext context, IUserService userService)
        {
            _logger = logger;
            _context = context;
            _userService = userService;
        }
        public IActionResult Dashboard()
        {
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
