using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GCTConnect.Models;
using GCTConnect.ViewModel.Admin;
using GCTConnect.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using Microsoft.AspNetCore.SignalR;
using GCTConnect.Models.ViewModels;
using System.Linq.Expressions;



namespace GCTConnect.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminController : Controller
{
    private readonly IUserService _userService;

    private readonly ILogger<AdminController> _logger;

    private readonly GctConnectContext _context;

    public AdminController(ILogger<AdminController> logger, GctConnectContext context, IUserService userService)
    {
        _logger = logger;
        _context = context;
        _userService = userService;
    }

    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]

    [HttpGet("")]
    public async Task<IActionResult> Dashboard()
    {
        var user = _userService.GetCurrentUser();
        if (user == null)
        {
            return RedirectToAction("Login", "Home");
        }
        // Stats
        ViewBag.ActivePage = "Dashboard";

        ViewBag.UserCount = await _context.Users.CountAsync();
        ViewBag.BatchCount = await _context.Batches
                      .Select(b => b.BatchYear)
                      .Distinct()
                      .CountAsync();

        ViewBag.DepartmentCount = await _context.Departments.CountAsync();
        ViewBag.GroupCount = await _context.Groups.CountAsync();

        // Recent Activities - latest 3 events
        var activities = new List<ActivityViewModel>();

        var latestUser = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new ActivityViewModel
            {
                Type = "user",
                Description = $"New user registered: {u.Name} {u.LastName}",
                TimeAgo = (DateTime)u.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (latestUser != null)
            activities.Add(latestUser);

        var latestBatch = await _context.Batches
            .OrderByDescending(b => b.BatchId)
            .Include(b => b.Department)
            .Select(b => new ActivityViewModel
            {
                Type = "batch",
                Description = $"Batch {b.BatchYear} created successfully",
                TimeAgo = DateTime.Now // Assuming batch creation time isn't stored
            })
            .FirstOrDefaultAsync();

        if (latestBatch != null)
            activities.Add(latestBatch);

        var latestAnnouncement = await _context.Announcements
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new ActivityViewModel
            {
                Type = "announcement",
                Description = $"New announcement posted",
                TimeAgo = (DateTime)a.CreatedAt
            })
            .FirstOrDefaultAsync();

        var latestGroup = await _context.Groups
              .OrderByDescending(a => a.CreatedAt)
              .Select(a => new ActivityViewModel
              {
                  Type = "group",
                  Description = $"New Group Added",
                  TimeAgo = (DateTime)a.CreatedAt
              })
              .FirstOrDefaultAsync();
        if (latestGroup != null)
            activities.Add(latestGroup);

        ViewBag.Activities = activities;

        // System Overview (placeholder - add logic later)
        ViewBag.ActiveUsers = await _context.Users.CountAsync(); // Add status if available
        ViewBag.PendingApprovals = 0; // Add your logic later
        ViewBag.SystemHealth = "Good"; // Static for now
        ViewBag.StorageUsed = "68%"; // Static or calculate based on file table

        return View();
    }

    //[HttpGet("user-management")]
    //public IActionResult UserManagement()
    //{
    //    var currentUser = _userService.GetCurrentUser();
    //    if (currentUser == null)
    //    {
    //        return NotFound();
    //    }
    //    string currentUserRole = currentUser.Role;
    //    ViewData["CurrentUserRole"] = currentUserRole;
    //    ViewBag.Batches = _context.Batches
    //        .GroupBy(b => b.BatchYear) // Group batches by year
    //        .Select(g => new
    //        {
    //            Id = g.First().BatchId,  // Use the first BatchId in the group
    //            Year = g.Key              // The key is the BatchYear
    //        })
    //        .ToList(); // Materialize the results in memory
    //    ViewBag.Departments = _context.Departments.Select(d => new { Id = d.DepartmentId, Name = d.Name }).ToList();

    //    ViewBag.Subjects = _context.Users
    //          .Where(u => u.Subject != null) // Exclude NULL entries
    //          .Select(u => u.Subject)
    //          .Distinct()
    //          .ToList();



    //    ViewBag.ActivePage = "UserManagement";

    //    var users = _context.Users.Include(u => u.Departments).ToList();
    //    ViewBag.DepartmentNames = _context.Departments.ToList();
    //    return View(users);
    //}



    [HttpGet("user-management")]
    public IActionResult UserManagement(string role = "", int departmentId = 0, string search = "", int page = 1, int pageSize = 10)
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

        ViewBag.ActivePage = "UserManagement";

        // Start with all users
        var usersQuery = _context.Users.Include(u => u.Departments).AsQueryable();

        // Apply role filter
        if (!string.IsNullOrEmpty(role))
        {
            usersQuery = usersQuery.Where(u => u.Role.ToLower() == role.ToLower());
        }

        // Apply department filter
        if (departmentId > 0)
        {
            usersQuery = usersQuery.Where(u => u.DepartmentId == departmentId);
        }

        // Apply search filter
        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            usersQuery = usersQuery.Where(u =>
                u.Name.ToLower().Contains(search) ||
                u.LastName.ToLower().Contains(search) ||
                u.Email.ToLower().Contains(search) ||
                (u.RollNumber != null && u.RollNumber.ToLower().Contains(search))
            );
        }

        // Get total count for pagination
        var totalUsers = usersQuery.Count();
        var totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

        // Apply pagination
        var users = usersQuery
            .OrderBy(u => u.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Pass pagination and filter data to view
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalUsers = totalUsers;
        ViewBag.CurrentRole = role;
        ViewBag.CurrentDepartmentId = departmentId;
        ViewBag.CurrentSearch = search;

        ViewBag.DepartmentNames = _context.Departments.ToList();
        return View(users);
    }
    [HttpPost]
    public IActionResult AddUser(User user)
    {
        if (ModelState.IsValid)
        {
            // Example: Hash password before saving
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();
            return RedirectToAction("UserManagement");
        }

        ViewBag.Departments = _context.Departments.ToList();
        var users = _context.Users.Include(u => u.Departments).ToList();
        return View("UserManagement", users);
    }



    [HttpDelete("{id}")]
    [Route("delete-user/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.Where(u=>u.UserId == id)
            .FirstOrDefaultAsync();

        if (user == null)
            return NotFound(new { message = "User not found." });

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.AnnouncementRecipients.RemoveRange(
                _context.AnnouncementRecipients.Where(ar => ar.UserId == id));

            _context.ChatbotQueries.RemoveRange(
                _context.ChatbotQueries.Where(q => q.UserId == id));

            _context.Files.RemoveRange(
                _context.Files.Where(f => f.UploaderId == id));

            _context.Friends.RemoveRange(
                _context.Friends.Where(f => f.SenderId == id || f.ReceiverId == id));

            _context.GroupMembers.RemoveRange(
                _context.GroupMembers.Where(gm => gm.UserId == id));

            _context.Groups.RemoveRange(
                _context.Groups.Where(g => g.CreatedBy == id));

            _context.Messages.RemoveRange(
                _context.Messages.Where(m => m.SenderId == id));


            _context.UserProfiles.RemoveRange(
                _context.UserProfiles.Where(up => up.UserId == id));

            // 2. Nullify Department HOD if user is a HOD
            var departments = _context.Departments.Where(d => d.HodId == id);
            foreach (var dept in departments)
            {
                dept.HodId = null;
            }

            // 3. Finally delete the user
            _context.Users.Remove(user);

            // Commit changes
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { message = "User deleted successfully." });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { error = ex.Message });
        }
    }



    [HttpGet("batch-management")]
    public async Task<IActionResult> BatchManagement()
    {
        ViewBag.ActivePage = "BatchManagement";

        var batchSummary = await _context.Batches
            .GroupBy(b => b.BatchYear)
            .Select(g => new BatchViewModel
            {
                BatchYear = g.Key,
                StudentsCount = g.Sum(b => b.StudentsCount),
                DepartmentCount = g.Select(b => b.DepartmentId).Distinct().Count(),
                IsActive = true // You can add custom logic for active/inactive
            })
            .OrderByDescending(b => b.BatchYear)
            .ToListAsync();

        return View(batchSummary);
    }

    [HttpGet("group-management")]
    public async Task<IActionResult> GroupManagement()
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

        ViewBag.ActivePage = "GroupManagement";

        var groupsData = await _context.Groups
            .Include(g => g.Department)
            .ToListAsync(); // materialize here

        var groups = groupsData
            .Select(g => new GroupViewModel
            {
                GroupId = g.GroupId,
                GroupName = g.GroupName,
                DepartmentName = g.Department != null
                    ? g.Department.Name
                    : g.GroupName.Split('_')[1],
                MembersCount = _context.GroupMembers.Count(m => m.GroupId == g.GroupId),
                CreatedAt = g.CreatedAt ?? DateTime.Now
            })
            .OrderByDescending(g => g.CreatedAt)
            .ToList();

        return View(groups);
    }


    [HttpGet("department-management")]
    public async Task<IActionResult> DepartmentManagement()
    {
        ViewBag.ActivePage = "DepartmentManagement";

        var departments = await _context.Departments
            .Include(d => d.Hod) // only if you have navigation property
            .Select(d => new
            {
                d.DepartmentId,
                d.Name,
                HodName = _context.Users
                            .Where(u => u.UserId == d.HodId)
                            .Select(u => u.Name + " " + u.LastName)
                            .FirstOrDefault(),
                StudentsCount = _context.Users.Count(u => u.DepartmentId == d.DepartmentId && u.Role == "Students"),
                TeachersCount = _context.Users.Count(u => u.DepartmentId == d.DepartmentId && u.Role == "Teachers")
            })
            .ToListAsync(); // <-- materialize here

        var departmentVMs = departments.Select(d => new DepartmentViewModel
        {
            DepartmentId = d.DepartmentId,
            DepartmentName = d.Name,
            HodName = d.HodName ?? "N/A",
            StudentsCount = d.StudentsCount,
            TeachersCount = d.TeachersCount,
            IconClass = d.Name switch
            {
                "ComputerScience" => "fas fa-laptop-code",
                "Electronics" => "fas fa-microchip",
                _ => "fas fa-university"
            }
        }).ToList();


        return View(departmentVMs);
    }



    [HttpGet("announcement")]
    public IActionResult Announcement()
    {
        ViewBag.ActivePage = "Announcement";

        var announcements = _context.Announcements
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AnnouncementViewModel
            {
                AnnouncementId = a.AnnouncementId,
                Title = a.Title,
                Content = a.Content,
                CreatedAt = (DateTime)a.CreatedAt,
                Priority = a.Priority == 1 ? "Low Priority" :
                           a.Priority == 2 ? "Medium Priority" :
                           a.Priority == 3 ? "High Priority" : "Unknown",
                Audience = a.Audience == "all" ? "All" :
           a.Audience == "hods" ? "HODs" :
           a.Audience == "teachers_all" ? "All Teachers" :
           a.Audience == "teachers_department" ? "Teachers (Selected Department)" :
           a.Audience == "students_all" ? "All Students" :
           a.Audience == "students_department" ? "Students (Selected Department)" :
           a.Audience == "students_batch" ? "Students (Specific Batch)" :
           a.Audience == "students_department_batch" ? "Students (Department & Batch)" :
           "Unknown",
            })
            .ToList();

        ViewBag.Announcements = announcements;
        return View();
    }



    [HttpPost("create-announcement")]
    public async Task<IActionResult> CreateAnnouncement([FromBody] AnnouncementRequest request, [FromServices] IHubContext<ChatHub> hubContext)
    {
        var announcement = new Announcement
        {
            Title = request.Title,
            Content = request.Content,
            DepartmentId = request.DepartmentId,
            Priority = request.Priority == "Low" ? 1 :
                           request.Priority == "Medium" ? 2 :
                           request.Priority == "High" ? 3 : 0,
            Audience = request.Audience,
            CreatedAt = DateTime.Now
        };
        _context.Announcements.Add(announcement);
        await _context.SaveChangesAsync();

        // Get recipients
        IQueryable<User> recipients = request.Audience switch
        {
            "hods" => _context.Users.Where(u => u.Role == "Hod"),
            "teachers_all" => _context.Users.Where(u => u.Role == "Teachers"),
            "teachers_department" => _context.Users.Where(u => u.Role == "Teachers" && u.DepartmentId == request.DepartmentId),
            "students_all" => _context.Users.Where(u => u.Role == "Students"),
            "students_department" => _context.Users.Where(u => u.Role == "Students" && u.DepartmentId == request.DepartmentId),
            "students_batch" => _context.Users.Where(u => u.Role == "Students" && u.BatchId == request.BatchId),
            "students_department_batch" => _context.Users.Where(u => u.Role == "Students" && u.DepartmentId == request.DepartmentId && u.BatchId == request.BatchId),
            "all" => _context.Users,
            _ => Enumerable.Empty<User>().AsQueryable()
        };

        var recipientList = await recipients.ToListAsync();

        // Save recipients in DB and send via SignalR
        foreach (var recipient in recipientList)
        {
            _context.AnnouncementRecipients.Add(new AnnouncementRecipient
            {
                AnnouncementId = announcement.AnnouncementId,
                UserId = recipient.UserId
            });
        }

        await _context.SaveChangesAsync();

        
        var userIds = recipientList.Select(r => r.UserId).ToList();
        try
        {
            await ChatHub.SendAnnouncementToMultipleUsers(
                hubContext,
                _context,
                announcement.AnnouncementId,
                userIds,
                announcement.Title,
                announcement.Content,
                request.Priority
            );
        }
        catch (Exception ex){
            return NotFound();
        }
        return Ok(new { message = "Announcement created and sent!" });
    }


    [HttpGet("add-batch")]
    public IActionResult AddBatch()
    {

        List<Batch> batchDetails = _context.Batches.Include((Batch u) => u.Department).ToList();
        base.ViewBag.BatchDetails = batchDetails;
        return View();
    }

    [HttpPost]
    public JsonResult AddBatch(int newBatch)
    {
        if (_userService.GetCurrentUser() == null)
        {
            return Json(new
            {
                success = false,
                message = "User is not authenticated."
            });
        }
        string pattern = "^20\\d{2}$";
        if (_context.Batches.Any((Batch u) => u.BatchYear == newBatch))
        {
            base.TempData["ErrorMessage"] = "This batch is already exists!";
            return Json(new
            {
                success = false
            });
        }
        if (!Regex.IsMatch(newBatch.ToString(), pattern))
        {
            base.TempData["ErrorMessage"] = "An error occurred while saving the batch " + newBatch;
            return Json(new
            {
                success = false
            });
        }
        List<Department> departments = _context.Departments.ToList();
        User currentUser = _userService.GetCurrentUser();
        foreach (Department department in departments)
        {
            Batch batch = new Batch
            {
                BatchYear = newBatch,
                DepartmentId = department.DepartmentId,
                StudentsCount = 0
            };
            _context.Batches.Add(batch);
            GCTConnect.Models.Group studentsOnlyGroup = new GCTConnect.Models.Group
            {
                GroupName = $"{newBatch}_{department.Name}_StudentsOnly",
                Description = $"A group only for students batch: {newBatch} & department: {department.Name}",
                CreatedBy = currentUser.UserId,
                DepartmentId = department.DepartmentId,
                CreatedAt = DateTime.UtcNow
            };
            GCTConnect.Models.Group studentsTeachersHodGroup = new GCTConnect.Models.Group
            {
                GroupName = $"{newBatch}_{department.Name}_StudentsTeachersHod",
                Description = $"A group only for students, teachers, and Hod of batch: {newBatch} & department: {department.Name}",
                CreatedBy = currentUser.UserId,
                DepartmentId = department.DepartmentId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Groups.AddRange(studentsOnlyGroup, studentsTeachersHodGroup);
        }
        _context.SaveChanges();
        base.TempData["Message"] = newBatch + " Batch has been saved successfully!";
        return Json(new
        {
            success = true
        });
    }

    [HttpPost]
    public JsonResult DeleteBatch(int batch)
    {
        List<Batch> deleteBatches = _context.Batches.Where((Batch u) => u.BatchYear == batch).ToList();
        if (deleteBatches == null)
        {
            base.TempData["ErrorMessage"] = "An error occurred while saving the batch " + batch;
            return Json(new
            {
                success = false
            });
        }
        List<GCTConnect.Models.Group> groups = _context.Groups.Where((GCTConnect.Models.Group g) => g.GroupName.Contains(Convert.ToString(batch))).ToList();
        foreach (GCTConnect.Models.Group group in groups)
        {
            List<GroupMember> deletedGroupMembers = _context.GroupMembers.Where((GroupMember u) => u.GroupId == (int?)group.GroupId).ToList();
            foreach (GroupMember deletedGrpupMember in deletedGroupMembers)
            {
                _context.GroupMembers.Remove(deletedGrpupMember);
            }
            _context.Groups.Remove(group);
            _context.SaveChanges();
        }
        foreach (Batch deleteBatch in deleteBatches)
        {
            List<User> users = _context.Users.Where((User u) => u.BatchId == (int?)deleteBatch.BatchId && u.DepartmentId == (int?)deleteBatch.DepartmentId).ToList();
            foreach (User user in users)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
            _context.Batches.Remove(deleteBatch);
            _context.SaveChanges();
        }
        List<Batch> batchDetails = _context.Batches.Include((Batch u) => u.Department).ToList();
        base.ViewBag.BatchDetails = batchDetails;
        base.TempData["Message"] = batch + " Batch has been deleted successfully!";
        return Json(new
        {
            success = true
        });
    }

    //public async Task<IActionResult> Dashboard()
    //{
    //    var usersCount = await _context.Users.CountAsync();
    //    ViewBag.UserCount = usersCount;
    //    return View();
    //}

}
