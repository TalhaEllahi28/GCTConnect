using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GCTConnect.Models;
using GCTConnect.Services;
namespace GCTConnect.Controllers.Api
{

    public class UserProfileController : Controller
    {
        private readonly GctConnectContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IUserService _userService;
        public UserProfileController(GctConnectContext context, IWebHostEnvironment environment, IUserService userService)
        {
            _context = context;
            _environment = environment;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> UserProfileManagement()
        {
            var user = _userService.GetCurrentUser();
            ViewBag.User = "Student";
            ViewData["CurrentUserRole"] = user.Role;
            ViewBag.ProfilePic = user.ProfilePic;

            if (user == null)
            {
                TempData["NotAuthorized"] = "You are not an authorized user please Login first!";
                return RedirectToAction("Home", "Login");
            }

            var departmentName = _context.Departments
                .Where(d => d.DepartmentId == user.DepartmentId)
                .Select(d => d.Name)
                .FirstOrDefault();
            if (departmentName == null) return NotFound();

            var batchYear = _context.Batches
                .Where(b => b.BatchId == user.BatchId)
                .Select(b => b.BatchYear)
                .FirstOrDefault();
            if (batchYear == null) return NotFound();

            var viewModel = new UserProfileViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role,
                DepartmentName = departmentName,
                BatchName = Convert.ToString(batchYear),
                RollNumber = user.RollNumber,
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                Subject = user.Subject,
                Bio = user.UserProfile?.Bio,
                SocialLinks = user.UserProfile?.SocialLinks,
                CurrentProfilePicPath = user.ProfilePic
            };


            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserProfileManagement(UserProfileViewModel model)
        {
            var user = _userService.GetCurrentUser();

            if (user == null) return NotFound();

            var trackedUser = await _context.Users
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.UserId == user.UserId);

            if (trackedUser == null) return NotFound();

            trackedUser.Name = model.Name;
            trackedUser.LastName = model.LastName;
            trackedUser.Email = model.Email;
            trackedUser.PhoneNumber = model.PhoneNumber;
            trackedUser.Gender = model.Gender;
            trackedUser.Subject = model.Subject;

            if (model.CurrentProfilePicPath != null)
            {
                trackedUser.ProfilePic = model.CurrentProfilePicPath;
            }

            if (trackedUser.UserProfile == null)
            {
                trackedUser.UserProfile = new UserProfile
                {
                    UserId = trackedUser.UserId,
                    Bio = model.Bio,
                    SocialLinks = model.SocialLinks,
                    UpdatedAt = DateTime.Now
                };
                _context.UserProfiles.Add(trackedUser.UserProfile);
            }
            else
            {
                trackedUser.UserProfile.Bio = model.Bio;
                trackedUser.UserProfile.SocialLinks = model.SocialLinks;
                trackedUser.UserProfile.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            ViewBag.ProfilePic = trackedUser.ProfilePic;
            TempData["Success"] = "Changes are successfully updated";
            return RedirectToAction("UserProfileManagement");
        }
    }
}
