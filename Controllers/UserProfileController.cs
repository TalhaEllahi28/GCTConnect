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

            if (user == null)
            {
                TempData["NotAuthorized"] = "You are not an authorized user please Login first!";
                return RedirectToAction("Home", "Login");
            }

            ViewData["CurrentUserRole"] = user.Role;
            ViewBag.ProfilePic = user.ProfilePic;
            ViewBag.User = user.Role;

            string departmentName = null;
            string batchYear = null;

            // Only fetch department data for roles that need it
            if (user.Role == "Students" || user.Role == "Teacher")
            {
                departmentName = await _context.Departments
                    .Where(d => d.DepartmentId == user.DepartmentId)
                    .Select(d => d.Name)
                    .FirstOrDefaultAsync();

                if (departmentName == null) return NotFound("Department not found");
            }

            // Only fetch batch data for Students
            if (user.Role == "Students")
            {
                var batch = await _context.Batches
                    .Where(b => b.BatchId == user.BatchId)
                    .Select(b => b.BatchYear)
                    .FirstOrDefaultAsync();

                batchYear = batch.ToString();
                if (batchYear == null) return NotFound("Batch not found");
            }

            // Create base viewModel with common properties
            var viewModel = new UserProfileViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role,
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                Bio = user.UserProfile?.Bio,
                SocialLinks = user.UserProfile?.SocialLinks,
                CurrentProfilePicPath = user.ProfilePic
            };

            // Add role-specific properties
            switch (user.Role)
            {
                case "Students":
                    viewModel.DepartmentName = departmentName;
                    viewModel.BatchName = batchYear;
                    viewModel.RollNumber = user.RollNumber;
                    break;

                case "Teacher":
                    viewModel.DepartmentName = departmentName;
                    viewModel.Subject = user.Subject;
                    break;


                case "HOD":
                    // HOD might have department info
                    if (user.DepartmentId.HasValue)
                    {
                        viewModel.DepartmentName = await _context.Departments
                            .Where(d => d.DepartmentId == user.DepartmentId)
                            .Select(d => d.Name)
                            .FirstOrDefaultAsync();
                    }
                    break;

                default:
                    break;
            }

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
