using GCTConnect.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using GCTConnect.Services;
using System.Linq;
using GCTConnect.Models.ViewModels;


namespace GCTConnect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EmailService _emailService;
        private readonly GctConnectContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;
        public HomeController(ILogger<HomeController> logger, GctConnectContext context, IHttpContextAccessor httpContextAccessor, EmailService emailService, IUserService userService)
        {
            _logger = logger;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _userService = userService;
        }



        [HttpGet("api/teachers-and-subjects")]
        public IActionResult GetTeachersAndSubjects(int departmentId)
        {
            var teachers = _context.Users
                .Where(u => u.DepartmentId == departmentId && (u.Role == "Teachers" || u.Role == "Hod"))
                .Select(u => new { Id = u.UserId, Name = u.Username })
                .ToList();

            var subjects = _context.Users
                .Where(u => u.DepartmentId == departmentId && u.Subject != null)
                .Select(u => u.Subject)
                .Distinct()
                .ToList();

            return Json(new { teachers, subjects });
        }


        [Authorize(Roles = "Admin,Hod")]
        public IActionResult CreateGroup()
        {
            ViewBag.Batches = _context.Batches
                .GroupBy(b => b.BatchYear) // Group batches by year
                .Select(g => new
                {
                    Id = g.First().BatchId,  // Use the first BatchId in the group
                    Year = g.Key              // The key is the BatchYear
                })
                .ToList(); // Materialize the results in memory
            ViewBag.Departments = _context.Departments.Select(d => new { Id = d.DepartmentId, Name = d.Name }).ToList();

            return View();
        }

        [Authorize(Roles = "Admin,Hod")]
        [HttpPost]
        public JsonResult CreateGroups([FromBody] CreateGroup newGroup)
        {
            var currentUser = _userService.GetCurrentUser();
            if (currentUser == null)
            {
                TempData["ErrorMessage"] = "User is not authenticated OR Your Session is expired please Login ";
                return Json(new
                {
                    success = false
                });
            }

            if (ModelState.IsValid)
            {
                int batch = _context.Batches.FirstOrDefault(b => b.BatchId == newGroup.BatchId).BatchYear;
                string department = _context.Departments.FirstOrDefault(d => d.DepartmentId == newGroup.StudentDepartmentId).Name;
                Group group = new Group
                {
                    CreatedBy = currentUser.UserId,
                    IsPrivate = false,
                    GroupName = $"{batch}_{department}_{newGroup.Subject}",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Groups.Add(group); 
                _context.SaveChanges();
                

                List<User> user = _context.Users.Where(u => u.BatchId==newGroup.BatchId && u.DepartmentId == newGroup.StudentDepartmentId).ToList();
                var Teacher = _context.Users.Where(u => u.UserId == newGroup.TeacherId && u.DepartmentId == newGroup.TeacherDepartmentId).FirstOrDefault();

                foreach (var item in user)
                {
                    GroupMember groupMember = new GroupMember
                    {
                        GroupId = group.GroupId,
                        UserId = item.UserId,
                        Role = "group_member",
                        JoinedAt = DateTime.UtcNow
                    };
                    _context.GroupMembers.Add(groupMember);
                    _context.SaveChanges();
                }
                GroupMember groupMember1 = new GroupMember
                {
                    GroupId = group.GroupId,
                    UserId = Teacher.UserId,
                    Role = "group_admin",
                    JoinedAt = DateTime.UtcNow
                };
                _context.GroupMembers.Add(groupMember1);
                _context.SaveChanges();
                ModelState.Clear();
                TempData["SuccessMessage"] = "Group is successfully created";

                return Json(new
                {
                    success = true
                });
            }

            return Json(new
            {
                success = false
            });
        }

        [Authorize(Roles = "Admin,Hod")]
        public IActionResult AddSomeone()
        {
            var currentUser = _userService.GetCurrentUser();
            if (currentUser == null)
            {
                return NotFound();
            }
            string currentUserRole = currentUser.Role;
            ViewData["CurrentUserRole"] = currentUserRole;
            ViewBag.Batches = _context.Batches
                .GroupBy(b => b.BatchYear) // Group batches by year
                .Select(g => new
                {
                    Id = g.First().BatchId,  // Use the first BatchId in the group
                    Year = g.Key              // The key is the BatchYear
                })
                .ToList(); // Materialize the results in memory
            ViewBag.Departments = _context.Departments.Select(d => new { Id = d.DepartmentId, Name = d.Name }).ToList();

            ViewBag.Subjects = _context.Users
                  .Where(u => u.Subject != null) // Exclude NULL entries
                  .Select(u => u.Subject)
                  .Distinct()
                  .ToList();

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Hod")]
        public async Task<IActionResult> AddSomeone(User newUser)
        {
            // Reload dropdown data in case of validation errors
            ViewBag.Batches = _context.Batches
                .Select(b => new { Id = b.BatchId, Year = b.BatchYear })
                .ToList();
            ViewBag.Departments = _context.Departments
                .Select(d => new { Id = d.DepartmentId, Name = d.Name })
                .ToList();
            ViewBag.Subjects = _context.Users
                .Where(u => u.Subject != null) // Exclude NULL entries
                .Select(u => u.Subject)
                .Distinct()
                .ToList();

            var currentUser = _userService.GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction("Login");
            }
            ModelState.Remove("Username");
            ModelState.Remove("Password");
            if (!ModelState.IsValid)
            {
                return View(newUser);
            }

            if (_context.Users.Any(u => u.Email == newUser.Email))
            {
                ModelState.AddModelError("Email", "This email already exists.");
                return View(newUser);
            }

            var User = new User
            {
                Name = newUser.Name,
                LastName = newUser.LastName,
                Email = newUser.Email,
                Gender = newUser.Gender,
                CreatedAt = DateTime.UtcNow,
                Password = GenerateComplexRandomPassword(8)
            };

            if (newUser.PhoneNumber != null)
            {
                User.PhoneNumber = newUser.PhoneNumber;
            }

            // Role-based logic
            if ((newUser.Role == "Students" && (currentUser.Role == "Admin" || currentUser.Role == "Hod")))
            {
                User.Role = newUser.Role;

                if (newUser.RollNumber == null)
                {
                    ModelState.AddModelError("RollNumber", "Roll Number is required");
                    return View(newUser);
                }

                if (newUser.DepartmentId == null)
                {
                    ModelState.AddModelError("Department", "Department Name is required");
                    return View(newUser);
                }

                if (newUser.BatchId == null)
                {
                    ModelState.AddModelError("Batch", "Batch Year is required");
                    return View(newUser);
                }

                User.RollNumber = newUser.RollNumber;
                User.Username = $"{newUser.Name}-{newUser.RollNumber}";
                var batchYear = _context.Batches.FirstOrDefault(b => b.BatchId == newUser.BatchId)?.BatchYear;
                User.BatchId = _context.Batches.FirstOrDefault(b => b.DepartmentId == newUser.DepartmentId && b.BatchYear == batchYear)?.BatchId;
                User.DepartmentId = newUser.DepartmentId;

                var batch = _context.Batches.FirstOrDefault(b => b.BatchId == User.BatchId);
                var department = _context.Departments.FirstOrDefault(d => d.DepartmentId == newUser.DepartmentId);
                // Save user to the database
                _context.Users.Add(User);
                _context.SaveChanges();
                if (batch != null)
                {
                    batch.StudentsCount += 1; // Increment student count
                    _context.SaveChanges(); // Save changes
                }


                addUserInGroup(batch, department, User, "StudentsOnly");
                addUserInGroup(batch, department, User, "StudentsTeachersHod");

            }
            else if (newUser.Role == "Teachers" && (currentUser.Role == "Admin" || currentUser.Role == "Hod"))
            {
                User.Role = newUser.Role;
                User.Username = newUser.Name;
                User.BatchId = null;
                User.DepartmentId = newUser.DepartmentId;
                User.Subject = newUser.Subject;
                // Save user to the database
                _context.Users.Add(User);
                _context.SaveChanges();
                var department = _context.Departments.FirstOrDefault(d => d.DepartmentId == newUser.DepartmentId);
                addUserInGroup(department, User, "StudentsTeachersHod");
                addUserInGroup(department, User, "TeachersHod");
            }

            else if ((newUser.Role == "Principal" || newUser.Role == "Hod" || newUser.Role == "Admin") &&
                     currentUser.Role == "Admin")
            {
                User.Role = newUser.Role;
                User.Username = newUser.Name;
                User.BatchId = null;
                User.DepartmentId = null;
                // Save user to the database
                _context.Users.Add(User);
                _context.SaveChanges();
                if (newUser.Role == "Hod")
                {
                    var department = _context.Departments.FirstOrDefault(d => d.DepartmentId == newUser.DepartmentId);
                    addUserInGroup(department, User, "StudentsTeachersHod");
                    addUserInGroup(User, "PrincipalHod");
                }
                else if (newUser.Role == "Principle")
                {
                    addUserInGroup(User, "PrincipalOnly");
                }
            }



            // Send email to the user with credentials
            try
            {
                var emailSender = HttpContext.RequestServices.GetRequiredService<EmailService>();
                string subject = "Your Account Registration Details";
                string body = $@"
            <h1>Welcome {User.Name},</h1>
            <p>Your account has been successfully registered. Below are your login details:</p>
            <p><strong>Username:</strong> {User.Username}</p>
            <p><strong>Password:</strong> {User.Password}</p>
            <p>Thank you!</p>";

                await emailSender.SendEmailAsync(User.Email, subject, body);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Failed to send email. Error: {ex.Message}");
                return View(newUser); // Redirect back to view in case of email failure
            }

            // Redirect to appropriate dashboard
            return currentUser.Role == "Admin" ? RedirectToAction("Dashboard", "Admin") : RedirectToAction("Dashboard", "Hod");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(User loginRequest)
        {
            // Fetch user from the database
            var user = _context.Users
                .FirstOrDefault(u => u.Username == loginRequest.Username && u.Password == loginRequest.Password);

            if (user == null)
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View();
                }
            // Create user claims
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role) // Ensure roles like Admin/Hod are here
        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Persist cookie across sessions
                ExpiresUtc = DateTime.UtcNow.AddMinutes(30) // Set expiration
            };

            // Sign in the user
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);





            // Redirect based on the user's role retrieved from the database
            switch (user.Role.ToLower())
            {
                case "students":
                    return RedirectToAction("Dashboard", "Student");
                case "teachers":
                    return RedirectToAction("Dashboard", "Teacher");
                case "hod":
                    return RedirectToAction("Dashboard", "Hod");
                case "principal":
                    return RedirectToAction("Dashboard", "Principal");
                case "admin":
                    return RedirectToAction("Dashboard", "Admin");
                default:
                    ModelState.AddModelError("", "Unknown role. Please contact the administrator.");
                    return View();
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        private void addUserInGroup(Batch batch, Department department, User user, string groupType)
        {
            var Group = _context.Groups.FirstOrDefault(u => u.GroupName == $"{batch.BatchYear}_{department.Name}_{groupType}");
            GroupMember groupMember = new GroupMember
            {
                GroupId = Group.GroupId,
                UserId = user.UserId,
                Role = "group_member",
                JoinedAt = DateTime.UtcNow,
            };
            _context.GroupMembers.Add(groupMember);
            _context.SaveChanges();
        }

        private void addUserInGroup(Department department, User user, string groupType)
        {
            // Get all groups where the department and groupType match after splitting the GroupName
            var groups = _context.Groups
                                 .AsEnumerable() // Switch to LINQ to Objects for string.Split
                                 .Where(g => {
                                     var parts = g.GroupName.Split('_');
                                     return parts.Length >= 3
                                            && parts[1].Equals(department.Name, StringComparison.OrdinalIgnoreCase)
                                            && parts[2].Equals(groupType, StringComparison.OrdinalIgnoreCase);
                                 })
                                 .ToList();

            // Add the user to each matching group
            foreach (var group in groups)
            {
                GroupMember groupMember = new GroupMember
                {
                    GroupId = group.GroupId,
                    UserId = user.UserId,
                    JoinedAt = DateTime.UtcNow,
                    Role = (user.Role == "Teachers" || user.Role == "Hod") ? "group_admin" : null
                };

                _context.GroupMembers.Add(groupMember);
            }
            _context.SaveChanges();
        }

        private void addUserInGroup(User user, string groupType)
        {
            var Group = _context.Groups.FirstOrDefault(u => u.GroupName == $"{groupType}");
            GroupMember groupMember = new GroupMember
            {
                GroupId = Group.GroupId,
                UserId = user.UserId,
                Role = "group_member",
                JoinedAt = DateTime.UtcNow,
            };
            _context.GroupMembers.Add(groupMember);
            _context.SaveChanges();
        }

        public static string GenerateComplexRandomPassword(int length)
        {
            if (length < 8 || length > 16)
                throw new ArgumentOutOfRangeException("Password length must be between 8 and 16 characters.");

            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string specialChars = "!@#$%^&*()-_=+";

            Random random = new Random();

            // Ensure the password has at least one lowercase, one uppercase, one digit, and one special character
            StringBuilder password = new StringBuilder();
            password.Append(lowercase[random.Next(lowercase.Length)]);
            password.Append(uppercase[random.Next(uppercase.Length)]);
            password.Append(digits[random.Next(digits.Length)]);
            password.Append(specialChars[random.Next(specialChars.Length)]);

            // Fill the rest of the password with random characters from all sets
            string allChars = lowercase + uppercase + digits + specialChars;
            for (int i = password.Length; i < length; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Shuffle the characters to ensure the password is random
            return new string(password.ToString().OrderBy(c => random.Next()).ToArray());
        }

        //public async Task<IActionResult> NotifyUser(string userEmail, string userName)
        //{
        //    string subject = "Welcome to GCT Connect!";
        //    string body = $"Dear {userName},<br><br>Welcome to our platform. Your account has been created successfully!<br>";

        //    bool isEmailSent = await _emailService.SendEmailAsync(userEmail, subject, body);

        //    if (isEmailSent)
        //    {
        //        return Ok("Email sent successfully!");
        //    }

        //    return BadRequest("Email sending failed.");
        //}
    }
}
