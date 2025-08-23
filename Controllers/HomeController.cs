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
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Data;


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

        [HttpGet]
        public async Task<IActionResult> GetUnreadAnnouncements()
        {
            var user = _userService.GetCurrentUser();
            if (user == null) return Unauthorized();

            var announcements = await _context.AnnouncementRecipients
                .Include(ar => ar.Announcement)
                .Where(ar => ar.UserId == user.UserId && ar.IsRead == false)
                .OrderByDescending(ar => ar.Announcement.CreatedAt)
                .Select(ar => new
                {
                    ar.Announcement.Title,
                    ar.Announcement.Content,
                    Priority = ar.Announcement.Priority == 1 ? "Low" :
                              ar.Announcement.Priority == 2 ? "Medium" : "High",
                    ar.Announcement.CreatedAt
                }).Take(3)
                .ToListAsync();

            return Ok(announcements);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead()
        {
            var user = _userService.GetCurrentUser();
            if (user == null) return Unauthorized();

            var unread = await _context.AnnouncementRecipients
                .Where(ar => ar.UserId == user.UserId && ar.IsRead == false)
                .ToListAsync();

            foreach (var item in unread)
            {
                item.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        public IActionResult ChatbotView()
        {

            return View();
        }

        [HttpGet]
        //[Route("GetTeachersAndSubjects")]
        //[Route("api/teachers-and-subjects")]
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

        [HttpGet]
        //[Route("GetCourses")]
        //[Route("api/GetCourses")]
        public JsonResult GetCourses(int departmentId)
        {
            var courses = _context.Courses
                .Where(c => c.DepartmentId == departmentId)
                .Select(c => new
                {
                    id = c.CourseId,
                    name = c.CourseName
                }).ToList();
            return Json(courses);
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
                string subjectName = _context.Courses.FirstOrDefault(c => c.CourseId == newGroup.SubjectId).CourseName;
                Group group = new Group
                {
                    CreatedBy = currentUser.UserId,
                    GroupName = $"{batch}_{department}_{subjectName}",
                    CourseId = newGroup.SubjectId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Groups.Add(group);
                _context.SaveChanges();

                newGroup.BatchYear = _context.Batches
                                           .Where(b => b.BatchId == newGroup.BatchId)
                                           .Select(b => b.BatchYear)
                                           .FirstOrDefault();                // First, find all BatchIds that correspond to the desired BatchYear.
                // This handles the case where multiple batches might exist for the same year.
                var batchIds = _context.Batches
                                       .Where(b => b.BatchYear == newGroup.BatchYear && b.DepartmentId == newGroup.StudentDepartmentId)
                                       .Select(b => b.BatchId)
                                       .ToList();

                // Now, query the Users table to find users who have a BatchId that is in the list of batchIds,
                // and also match the StudentDepartmentId.
                List<User> user = _context.Users
                                           .Where(u => batchIds.Contains((int)u.BatchId) && u.DepartmentId == newGroup.StudentDepartmentId)
                                           .ToList(); var Teacher = _context.Users.Where(u => u.UserId == newGroup.TeacherId && u.DepartmentId == newGroup.TeacherDepartmentId).FirstOrDefault();

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
                Password = GenerateComplexRandomPassword(8),
                ProfilePic = "https://thumbs.dreamstime.com/b/default-avatar-profile-icon-vector-social-media-user-image-182145777.jpg"
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
                    //return RedirectToAction("UserManagement", "Admin", newUser);

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
            TempData["Success"] = "ok";
            // Redirect to appropriate dashboard
            return currentUser.Role == "Admin" ? RedirectToAction("UserManagement", "Admin") : RedirectToAction("Dashboard", "Hod");
        }
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Login()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.Session.Clear();

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

        private void addUserInSubjectGroups(User user)
        {
            var departement = _context.Departments
                .Where(d => d.DepartmentId == user.DepartmentId)
                .Select(u => u.Name)
                .FirstOrDefault();

            var batchYear = _context.Batches
                .Where(u => u.BatchId == user.BatchId)
                .Select(b => b.BatchYear)
                .FirstOrDefault();

            var groups = _context.Groups
                .AsEnumerable() // needed because EF Core cannot translate Split to SQL
                .Where(g =>
                {
                    var parts = g.GroupName.Split('_');
                    return parts.Length >= 3 &&
                           parts[0] == batchYear.ToString() &&
                           parts[1] == departement;
                })
                .ToList();
            foreach( var group in groups)
            {
                GroupMember newMember = new GroupMember
                {
                    GroupId = group.GroupId,
                    UserId = user.UserId,
                    JoinedAt = DateTime.Now,
                    Role = "group_member",

                };

                _context.GroupMembers.Add(newMember);
                _context.SaveChanges();
            }
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
                                 .Where(g =>
                                 {
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

        private void addUserToAppropriateGroups(User user, User currentUser)
        {
            if (user.Role == "Students" && (currentUser.Role == "Admin" || currentUser.Role == "Hod"))
            {
                var batch = _context.Batches.FirstOrDefault(b => b.BatchId == user.BatchId);
                var department = _context.Departments.FirstOrDefault(d => d.DepartmentId == user.DepartmentId);

                if (batch != null && department != null)
                {
                    //addUserInGroup(batch, department, user, "StudentsOnly");
                    //addUserInGroup(batch, department, user, "StudentsTeachersHod");
                    addUserInSubjectGroups(user);
                }
            }
            else if (user.Role == "Teachers" && (currentUser.Role == "Admin" || currentUser.Role == "Hod"))
            {
                var department = _context.Departments.FirstOrDefault(d => d.DepartmentId == user.DepartmentId);
                if (department != null)
                {
                    addUserInGroup(department, user, "StudentsTeachersHod");
                    addUserInGroup(department, user, "TeachersHod");
                }
            }
            else if ((user.Role == "Principal" || user.Role == "Hod" || user.Role == "Admin") && currentUser.Role == "Admin")
            {
                if (user.Role == "Hod")
                {
                    var department = _context.Departments.FirstOrDefault(d => d.DepartmentId == user.DepartmentId);
                    if (department != null)
                    {
                        addUserInGroup(department, user, "StudentsTeachersHod");
                        addUserInGroup(department, user, "TeachersHod");
                    }
                    addUserInGroup(user, "PrincipalHod");
                }
                else if (user.Role == "Principal")
                {
                    addUserInGroup(user, "PrincipalOnly");
                    addUserInGroup(user, "PrincipalHod");
                }
            }
        }

        private void removeUserFromAllGroups(int userId)
        {
            var userGroupMemberships = _context.GroupMembers.Where(gm => gm.UserId == userId).ToList();

            foreach (var membership in userGroupMemberships)
            {
                _context.GroupMembers.Remove(membership);
            }
            _context.SaveChanges();
        }


        private void removeUserFromGroupsByType(int userId, List<string> groupTypes)
        {
            var userGroupMemberships = _context.GroupMembers
                .Where(gm => gm.UserId == userId)
                .Include(gm => gm.Group)
                .Where(gm => groupTypes.Any(gt => gm.Group.GroupName.Contains(gt)))
                .ToList();

            foreach (var membership in userGroupMemberships)
            {
                _context.GroupMembers.Remove(membership);
            }
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


        // Add these methods to your existing controller

        // Method to remove user from all current groups


        // Extracted method to add user to appropriate groups based on role


        // GET method for Edit User
        [HttpGet]
        [Authorize(Roles = "Admin,Hod")]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = _userService.GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction("Login");
            }

            // Check permissions
            if (currentUser.Role == "Hod" && user.Role != "Students" && user.Role != "Teachers")
            {
                return Forbid(); // HOD can only edit Students and Teachers
            }

            // Populate ViewBag data
            ViewBag.Batches = _context.Batches
                .Select(b => new { Id = b.BatchId, Year = b.BatchYear, DepartmentId = b.DepartmentId })
                .ToList();
            ViewBag.Departments = _context.Departments
                .Select(d => new { Id = d.DepartmentId, Name = d.Name })
                .ToList();
            ViewBag.Subjects = _context.Users
                .Where(u => u.Subject != null)
                .Select(u => u.Subject)
                .Distinct()
                .ToList();

            return View(user);
        }


        // GET method to fetch user data for editing (AJAX endpoint)
        [HttpGet]
        [Authorize(Roles = "Admin,Hod")]
        public async Task<IActionResult> GetUserForEdit(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = _userService.GetCurrentUser();
            if (currentUser == null)
            {
                return Unauthorized();
            }



            // Return user data as JSON
            var userData = new
            {
                userId = user.UserId,
                name = user.Name,
                lastName = user.LastName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                gender = user.Gender,
                role = user.Role,
                rollNumber = user.RollNumber,
                subject = user.Subject,
                departmentId = user.DepartmentId,
                batchId = user.BatchId,
               
            };

            return Json(userData);
        }


        // POST method for Edit User
        [HttpPost]
        [Authorize(Roles = "Admin,Hod")]
        public async Task<IActionResult> EditUser(User updatedUser)
        {
            // Reload dropdown data in case of validation errors
            ViewBag.Batches = _context.Batches
                .Select(b => new { Id = b.BatchId, Year = b.BatchYear, DepartmentId = b.DepartmentId })
                .ToList();
            ViewBag.Departments = _context.Departments
                .Select(d => new { Id = d.DepartmentId, Name = d.Name })
                .ToList();
            ViewBag.Subjects = _context.Users
                .Where(u => u.Subject != null)
                .Select(u => u.Subject)
                .Distinct()
                .ToList();

            var currentUser = _userService.GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction("Login");
            }

            // Get the existing user from database
            var existingUser = _context.Users.FirstOrDefault(u => u.UserId == updatedUser.UserId);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Check permissions
            if (currentUser.Role == "Hod" && existingUser.Role != "Students" && existingUser.Role != "Teachers")
            {
                return Forbid();
            }

            // Remove validation for Username and Password since they shouldn't be edited directly
            ModelState.Remove("Username");
            ModelState.Remove("Password");

            if (!ModelState.IsValid)
            {
                return View(updatedUser);
            }

            // Check if email already exists (excluding current user)
            if (_context.Users.Any(u => u.Email == updatedUser.Email && u.UserId != updatedUser.UserId))
            {
                ModelState.AddModelError("Email", "This email already exists.");
                return View(updatedUser);
            }

            // Store old values for comparison
            var oldRole = existingUser.Role;
            var oldDepartmentId = existingUser.DepartmentId;
            var oldBatchId = existingUser.BatchId;

            // Update basic user information
            existingUser.Name = updatedUser.Name;
            existingUser.LastName = updatedUser.LastName;
            existingUser.Email = updatedUser.Email;
            existingUser.Gender = updatedUser.Gender;
            existingUser.PhoneNumber = updatedUser.PhoneNumber;
            existingUser.Subject = updatedUser.Subject;

            // Role-based validation and updates
            bool roleChanged = oldRole != updatedUser.Role;
            bool departmentChanged = oldDepartmentId != updatedUser.DepartmentId;
            bool batchChanged = oldBatchId != updatedUser.BatchId;

            if (updatedUser.Role == "Students" && (currentUser.Role == "Admin" || currentUser.Role == "Hod"))
            {
                // Validation for students
                if (updatedUser.RollNumber == null)
                {
                    ModelState.AddModelError("RollNumber", "Roll Number is required");
                    return View(updatedUser);
                }

                if (updatedUser.DepartmentId == null)
                {
                    ModelState.AddModelError("DepartmentId", "Department is required");
                    return View(updatedUser);
                }

                if (updatedUser.BatchId == null)
                {
                    ModelState.AddModelError("BatchId", "Batch is required");
                    return View(updatedUser);
                }

                // Check if roll number already exists (excluding current user)
                if (_context.Users.Any(u => u.RollNumber == updatedUser.RollNumber && u.UserId != updatedUser.UserId))
                {
                    ModelState.AddModelError("RollNumber", "This roll number already exists.");
                    return View(updatedUser);
                }

                existingUser.Role = updatedUser.Role;
                existingUser.RollNumber = updatedUser.RollNumber;
                existingUser.Username = $"{updatedUser.Name}-{updatedUser.RollNumber}";
                existingUser.BatchId = updatedUser.BatchId;
                existingUser.DepartmentId = updatedUser.DepartmentId;
                existingUser.Subject = null; // Students don't have subjects
            }
            else if (updatedUser.Role == "Teachers" && (currentUser.Role == "Admin" || currentUser.Role == "Hod"))
            {
                if (updatedUser.DepartmentId == null)
                {
                    ModelState.AddModelError("DepartmentId", "Department is required");
                    return View(updatedUser);
                }

                existingUser.Role = updatedUser.Role;
                existingUser.Username = updatedUser.Name;
                existingUser.BatchId = null;
                existingUser.DepartmentId = updatedUser.DepartmentId;
                existingUser.Subject = updatedUser.Subject;
                existingUser.RollNumber = null; // Teachers don't have roll numbers
            }
            else if ((updatedUser.Role == "Principal" || updatedUser.Role == "Hod" || updatedUser.Role == "Admin") &&
                     currentUser.Role == "Admin")
            {
                existingUser.Role = updatedUser.Role;
                existingUser.Username = updatedUser.Name;
                existingUser.BatchId = null;
                existingUser.RollNumber = null;
                existingUser.Subject = null;

                if (updatedUser.Role == "Hod")
                {
                    if (updatedUser.DepartmentId == null)
                    {
                        ModelState.AddModelError("DepartmentId", "Department is required for HOD");
                        return View(updatedUser);
                    }
                    existingUser.DepartmentId = updatedUser.DepartmentId;
                }
                else
                {
                    existingUser.DepartmentId = null;
                }
            }

            // Handle batch count adjustments for students
            if (oldRole == "Students" && batchChanged)
            {
                // Decrease count from old batch
                if (oldBatchId.HasValue)
                {
                    var oldBatch = _context.Batches.FirstOrDefault(b => b.BatchId == oldBatchId.Value);
                    if (oldBatch != null && oldBatch.StudentsCount > 0)
                    {
                        oldBatch.StudentsCount -= 1;
                    }
                }

                // Increase count in new batch
                if (updatedUser.BatchId.HasValue)
                {
                    var newBatch = _context.Batches.FirstOrDefault(b => b.BatchId == updatedUser.BatchId.Value);
                    if (newBatch != null)
                    {
                        newBatch.StudentsCount += 1;
                    }
                }
            }

            // Handle group membership changes
            if (roleChanged || departmentChanged || batchChanged)
            {
                // Remove user from all current groups
                removeUserFromAllGroups(existingUser.UserId);

                // Add user to new appropriate groups
                addUserToAppropriateGroups(existingUser, currentUser);
            }

            // Save changes
            try
            {
                _context.SaveChanges();
                TempData["Success"] = "User updated successfully";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating user: {ex.Message}");
                return View(updatedUser);
            }

            // Redirect to appropriate dashboard
            return currentUser.Role == "Admin" ? RedirectToAction("UserManagement", "Admin") : RedirectToAction("Dashboard", "Hod");
        }
    }
}
