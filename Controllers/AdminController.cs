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
using System.Threading.Tasks;



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
    public IActionResult UserManagement(string role = "", int departmentId = 0, string search = "", int page = 1, int pageSize = 20)
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
            .OrderByDescending(u => u.UserId)
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










    #region BatchManagement
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

    [HttpGet("get-departments")]
    public JsonResult GetDepartments()
    {
        try
        {
            var departments = _context.Departments
                .Select(d => new { d.DepartmentId, d.Name })
                .OrderBy(d => d.Name)
                .ToList();

            return Json(new { success = true, departments });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Failed to load departments" });
        }
    }

    [HttpGet("get-batch-departments")]
    public JsonResult GetBatchDepartments(int batchYear)
    {
        try
        {
            var existingDepartments = _context.Batches
                .Where(b => b.BatchYear == batchYear)
                .Select(b => new {
                    b.DepartmentId,
                    b.Department.Name,
                    b.StudentsCount
                })
                .ToList();

            return Json(new { success = true, departments = existingDepartments });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Failed to load batch departments" });
        }
    }

    [HttpPost("add-batch")]
    public JsonResult AddBatch(int batchYear, int[] selectedDepartments)
    {
        if (_userService.GetCurrentUser() == null)
        {
            return Json(new { success = false, message = "User is not authenticated." });
        }

        try
        {
            // Validate batch year format
            string pattern = "^20\\d{2}$";
            if (!Regex.IsMatch(batchYear.ToString(), pattern))
            {
                return Json(new { success = false, message = $"Invalid batch year format: {batchYear}" });
            }

            // Validate selected departments
            if (selectedDepartments == null || selectedDepartments.Length == 0)
            {
                return Json(new { success = false, message = "Please select at least one department" });
            }

            // Check if batch already exists for any selected department
            var existingBatches = _context.Batches
                .Where(b => b.BatchYear == batchYear && selectedDepartments.Contains(b.DepartmentId))
                .Select(b => b.Department.Name)
                .ToList();

            if (existingBatches.Any())
            {
                return Json(new
                {
                    success = false,
                    message = $"Batch {batchYear} already exists for: {string.Join(", ", existingBatches)}"
                });
            }

            var currentUser = _userService.GetCurrentUser();
            var departments = _context.Departments
                .Where(d => selectedDepartments.Contains(d.DepartmentId))
                .ToList();

            // Create batches for selected departments
            foreach (var department in departments)
            {
                var batch = new Batch
                {
                    BatchYear = batchYear,
                    DepartmentId = department.DepartmentId,
                    StudentsCount = 0
                };
                _context.Batches.Add(batch);

                // Create two groups for each batch-department combination
                var studentsOnlyGroup = new Models.Group
                {
                    GroupName = $"{batchYear}_{department.Name}_StudentsOnly",
                    Description = $"A group only for students batch: {batchYear} & department: {department.Name}",
                    CreatedBy = currentUser.UserId,
                    DepartmentId = department.DepartmentId,
                    CreatedAt = DateTime.UtcNow
                };

                var studentsTeachersHodGroup = new Models.Group
                {
                    GroupName = $"{batchYear}_{department.Name}_StudentsTeachersHod",
                    Description = $"A group for students, teachers, and HOD of batch: {batchYear} & department: {department.Name}",
                    CreatedBy = currentUser.UserId,
                    DepartmentId = department.DepartmentId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Groups.AddRange(studentsOnlyGroup, studentsTeachersHodGroup);
            }

            _context.SaveChanges();

            return Json(new
            {
                success = true,
                message = $"Batch {batchYear} created successfully for {departments.Count} department(s)"
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "An error occurred while creating the batch" });
        }
    }

    [HttpPost("edit-batch")]
    public JsonResult EditBatch(int batchYear, int[] selectedDepartments)
    {
        if (_userService.GetCurrentUser() == null)
        {
            return Json(new { success = false, message = "User is not authenticated." });
        }

        try
        {
            // Validate selected departments
            if (selectedDepartments == null || selectedDepartments.Length == 0)
            {
                return Json(new { success = false, message = "Please select at least one department to add" });
            }

            // Get existing departments for this batch
            var existingDepartmentIds = _context.Batches
                .Where(b => b.BatchYear == batchYear)
                .Select(b => b.DepartmentId)
                .ToList();

            // Filter out departments that already exist for this batch
            var newDepartmentIds = selectedDepartments.Except(existingDepartmentIds).ToList();

            if (!newDepartmentIds.Any())
            {
                return Json(new
                {
                    success = false,
                    message = "All selected departments already exist for this batch"
                });
            }

            var currentUser = _userService.GetCurrentUser();
            var newDepartments = _context.Departments
                .Where(d => newDepartmentIds.Contains(d.DepartmentId))
                .ToList();

            // Create batches only for new departments
            foreach (var department in newDepartments)
            {
                var batch = new Batch
                {
                    BatchYear = batchYear,
                    DepartmentId = department.DepartmentId,
                    StudentsCount = 0  // New departments start with 0 students
                };
                _context.Batches.Add(batch);

                // Create groups for new departments only
                var studentsOnlyGroup = new Models.Group
                {
                    GroupName = $"{batchYear}_{department.Name}_StudentsOnly",
                    Description = $"A group only for students batch: {batchYear} & department: {department.Name}",
                    CreatedBy = currentUser.UserId,
                    DepartmentId = department.DepartmentId,
                    CreatedAt = DateTime.UtcNow
                };

                var studentsTeachersHodGroup = new Models.Group
                {
                    GroupName = $"{batchYear}_{department.Name}_StudentsTeachersHod",
                    Description = $"A group for students, teachers, and HOD of batch: {batchYear} & department: {department.Name}",
                    CreatedBy = currentUser.UserId,
                    DepartmentId = department.DepartmentId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Groups.AddRange(studentsOnlyGroup, studentsTeachersHodGroup);
            }

            _context.SaveChanges();

            return Json(new
            {
                success = true,
                message = $"Added {newDepartments.Count} new department(s) to batch {batchYear}"
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "An error occurred while editing the batch" });
        }
    }

    [HttpPost("check-batch-exists")]
    public JsonResult CheckBatchExists(int batchYear)
    {
        try
        {
            var batchExists = _context.Batches.Any(b => b.BatchYear == batchYear);
            return Json(new { success = true, exists = batchExists });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Error checking batch existence" });
        }
    }



    [HttpGet("get-batch-detail")]
    public JsonResult GetBatchDetails(int batchYear)
    {
        if (_userService.GetCurrentUser() == null)
        {
            return Json(new { success = false, message = "User is not authenticated." });
        }

        var batchDepartments = _context.Batches
            .Where(b => b.BatchYear == batchYear)
            .Select(b => b.DepartmentId)
            .ToList();

        var allDepartments = _context.Departments
            .Select(d => new { d.DepartmentId, d.Name })
            .ToList();

        return Json(new
        {
            success = true,
            selectedDepartments = batchDepartments,
            allDepartments = allDepartments
        });
    }

    //[HttpPost("add-or-edit")]
    //public JsonResult AddOrEditBatch(int batchYear, List<int> selectedDepartments)
    //{
    //    if (_userService.GetCurrentUser() == null)
    //    {
    //        return Json(new
    //        {
    //            success = false,
    //            message = "User is not authenticated."
    //        });
    //    }

    //    string pattern = "^20\\d{2}$";
    //    if (!Regex.IsMatch(batchYear.ToString(), pattern))
    //    {
    //        TempData["ErrorMessage"] = $"Invalid batch year format: {batchYear}. Year should be in 20XX format.";
    //        return Json(new { success = false });
    //    }

    //    if (selectedDepartments == null || !selectedDepartments.Any())
    //    {
    //        TempData["ErrorMessage"] = "Please select at least one department for the batch.";
    //        return Json(new { success = false });
    //    }

    //    try
    //    {
    //        User currentUser = _userService.GetCurrentUser();

    //        // Get existing batch-department combinations for this batch year
    //        var existingBatchDepartments = _context.Batches
    //            .Where(b => b.BatchYear == batchYear)
    //            .Select(b => b.DepartmentId)
    //            .ToList();

    //        // Find departments that need to be added (new departments for this batch)
    //        var departmentsToAdd = selectedDepartments.Except(existingBatchDepartments).ToList();

    //        if (!departmentsToAdd.Any() && existingBatchDepartments.Any())
    //        {
    //            TempData["ErrorMessage"] = "All selected departments already exist for this batch.";
    //            return Json(new { success = false });
    //        }

    //        // Get department details for the new departments
    //        var departments = _context.Departments
    //            .Where(d => departmentsToAdd.Contains(d.DepartmentId))
    //            .ToList();

    //        // Create batch records for new departments
    //        foreach (var department in departments)
    //        {
    //            // Create batch record
    //            Batch batch = new Batch
    //            {
    //                BatchYear = batchYear,
    //                DepartmentId = department.DepartmentId,
    //                StudentsCount = 0
    //            };
    //            _context.Batches.Add(batch);

    //            // Create StudentsOnly group
    //            Models.Group studentsOnlyGroup = new Models.Group
    //            {
    //                GroupName = $"{batchYear}{department.Name}StudentsOnly",
    //                Description = $"A group only for students batch: {batchYear} & department: {department.Name}",
    //                CreatedBy = currentUser.UserId,
    //                DepartmentId = department.DepartmentId,
    //                CreatedAt = DateTime.UtcNow
    //            };

    //            // Create StudentsTeachersHod group
    //            Models.Group studentsTeachersHodGroup = new Models.Group
    //            {
    //                GroupName = $"{batchYear}{department.Name}_StudentsTeachersHod",
    //                Description = $"A group only for students, teachers, and Hod of batch: {batchYear} & department: {department.Name}",
    //                CreatedBy = currentUser.UserId,
    //                DepartmentId = department.DepartmentId,
    //                CreatedAt = DateTime.UtcNow
    //            };

    //            _context.Groups.AddRange(studentsOnlyGroup, studentsTeachersHodGroup);
    //        }

    //        _context.SaveChanges();

    //        string action = existingBatchDepartments.Any() ? "updated" : "created";
    //        TempData["Message"] = $"Batch {batchYear} has been {action} successfully!";

    //        return Json(new { success = true });
    //    }
    //    catch (Exception ex)
    //    {
    //        TempData["ErrorMessage"] = $"An error occurred while processing the batch: {ex.Message}";
    //        return Json(new { success = false });
    //    }
    //}

    [HttpGet("get-batch-department-details")]
    public JsonResult GetBatchDepartmentDetails(int batchYear)
    {
        if (_userService.GetCurrentUser() == null)
        {
            return Json(new { success = false, message = "User is not authenticated." });
        }

        var batchDepartments = _context.Batches
            .Where(b => b.BatchYear == batchYear)
            .Include(b => b.Department)
            .Select(b => new {
                b.BatchId,
                b.DepartmentId,
                DepartmentName = b.Department.Name,
                b.StudentsCount
            })
            .ToList();

        return Json(new
        {
            success = true,
            batchYear = batchYear,
            departments = batchDepartments
        });
    }

    [HttpPost("delete-batch-department")]
    public JsonResult DeleteBatchDepartment(int batchYear, int departmentId)
    {
        if (_userService.GetCurrentUser() == null)
        {
            return Json(new { success = false, message = "User is not authenticated." });
        }

        try
        {
            // Find the specific batch-department combination
            var batchToDelete = _context.Batches
                .FirstOrDefault(b => b.BatchYear == batchYear && b.DepartmentId == departmentId);

            if (batchToDelete == null)
            {
                TempData["ErrorMessage"] = "Batch department combination not found.";
                return Json(new { success = false });
            }

            // Get department name for group deletion
            var department = _context.Departments.FirstOrDefault(d => d.DepartmentId == departmentId);
            if (department == null)
            {
                TempData["ErrorMessage"] = "Department not found.";
                return Json(new { success = false });
            }

            // Delete associated groups for this specific batch-department
            //var groupsToDelete = _context.Groups
            //    .Where(g => g.DepartmentId == departmentId &&
            //               (g.GroupName.Contains($"{batchYear}_{department.Name}_StudentsOnly") ||
            //                g.GroupName.Contains($"{batchYear}_{department.Name}_StudentsTeachersHod")))
            //    .ToList();

            List<GCTConnect.Models.Group> groupsToDelete = _context.Groups.Where((GCTConnect.Models.Group g) => g.GroupName.Contains($"{batchYear}_{department.Name}_")).ToList();

            foreach (var group in groupsToDelete)
            {
                // Delete group members first
                var groupMembers = _context.GroupMembers
                    .Where(gm => gm.GroupId == group.GroupId)
                    .ToList();

                _context.GroupMembers.RemoveRange(groupMembers);
                _context.Groups.Remove(group);
            }

            // Delete users associated with this specific batch-department
            var usersToDelete = _context.Users
                .Where(u => u.BatchId == batchToDelete.BatchId && u.DepartmentId == departmentId)
                .ToList();

            _context.Users.RemoveRange(usersToDelete);

            // Finally delete the batch record
            _context.Batches.Remove(batchToDelete);

            _context.SaveChanges();

            TempData["Message"] = $"{department.Name} has been removed from Batch {batchYear} successfully!";
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred while deleting the department: {ex.Message}";
            return Json(new { success = false });
        }
    }

    [HttpPost("delete-batch")]
    public JsonResult DeleteBatch(int batch)
    {
        List<Batch> deleteBatches = _context.Batches.Where((Batch u) => u.BatchYear == batch).ToList();
        if (deleteBatches == null)
        {
            base.TempData["ErrorMessage"] = "An error occurred while saving the batch " + batch;
            return Json(new
            {
                success = false,
                message = "An error occurred while saving the batch"+ batch
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
            success = true,
            message = $"Batch {batch} successfully deleted!"
        });
    }


    #endregion BatchManagement

















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
                    : (g.GroupName.Contains('_') && g.GroupName.Split('_').Length > 1
                        ? g.GroupName.Split('_')[1]
                        : "N/A"), // fallback value
                MembersCount = _context.GroupMembers.Count(m => m.GroupId == g.GroupId),
                CreatedAt = g.CreatedAt ?? DateTime.Now
            })
            .OrderByDescending(g => g.CreatedAt)
            .ToList();


        return View(groups);
    }

    [HttpPost("group-managment/delete/{id}")]
    public async Task<IActionResult> DeleteGroup(int id)
    {
        try
        {
            // Find group
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == id);
            if (group == null)
            {
                return Json(new { success = false, message = "Group not found." });
            }

            // Remove related messages
            var messages = await _context.Messages.Where(m => m.GroupId == id).ToListAsync();
            if (messages.Any())
            {
                _context.Messages.RemoveRange(messages);
            }

            // Remove related members
            var groupMembers = await _context.GroupMembers.Where(gm => gm.GroupId == id).ToListAsync();
            if (groupMembers.Any())
            {
                _context.GroupMembers.RemoveRange(groupMembers);
            }

            // Remove group itself
            _context.Groups.Remove(group);

            await _context.SaveChangesAsync();

            // Return success response for toast
            return Json(new { success = true, message = "Group deleted successfully!" });
        }
        catch (Exception ex)
        {
            // Log the error (optional)
            Console.WriteLine($"Error deleting group: {ex.Message}");

            return Json(new { success = false, message = "An error occurred while deleting the group." });
        }
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

    [HttpPost("add-department")]
    public async Task<JsonResult> AddDepartment(string department)
    {

        if (department == null)
        {
            return Json(new
            {
                success = false,
                message = "Invalid Department!"
            });
        }
        var newDepartment = new Department
        {
            Name = department
        };
        await _context.Departments.AddAsync(newDepartment);
        await _context.SaveChangesAsync();
        return Json(new
        {
            success = true,
            Department = department,
            message = "Department fetched successfully"
        });
    }


    [HttpGet("edit-department")]
    public async Task<JsonResult> EditDepartment(int depId)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.DepartmentId == depId);

        if (department == null)
        {
            return Json(new
            {
                success = false,
                message = "Invalid Department!"
            });
        }

        return Json(new
        {
            success = true,
            Department = department,
            message = "Department fetched successfully"
        });
    }

    [HttpPost("edit-department")]
    public async Task<JsonResult> EditDepartment(int depId, string name)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.DepartmentId == depId);

        if (department == null)
        {
            return Json(new
            {
                success = false,
                message = "Invalid Department!"
            });
        }

        var oldName = department.Name;
        department.Name = name;
        _context.Departments.Update(department);
        var groups = await _context.Groups
            .Where(g => g.GroupName.Contains($"_{oldName}_"))
            .ToListAsync();

        foreach (var group in groups)
        {
            group.GroupName = group.GroupName.Replace($"_{oldName}_", $"_{name}_");
            _context.Groups.Update(group);
        }

        await _context.SaveChangesAsync();

        return Json(new
        {
            success = true,
            Department = new
            {
                department.DepartmentId,
                department.Name
            },
            message = "Department and related groups updated successfully"
        });
    }

    [HttpDelete("delete-department")]
    public async Task<JsonResult> DeleteDepartment(int depId)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.DepartmentId == depId);

        if (department == null)
        {
            return Json(new
            {
                success = false,
                message = "Invalid Department!"
            });
        }

        // Find all groups for this department
        var groups = await _context.Groups
            .Where(g => g.GroupName.Contains($"_{department.Name}_"))
            .ToListAsync();

        var groupIds = groups.Select(g => g.GroupId).ToList();

        // Delete messages in those groups
        if (groupIds.Any())
        {
            var messages = await _context.Messages
                .Where(m => m.GroupId.HasValue && groupIds.Contains(m.GroupId.Value))
                .ToListAsync();

            _context.Messages.RemoveRange(messages);

            // Delete group members
            var usersInGroups = await _context.GroupMembers
                .Where(gm => groupIds.Contains((int)gm.GroupId))
                .ToListAsync();

            _context.GroupMembers.RemoveRange(usersInGroups);

            // Delete groups
            _context.Groups.RemoveRange(groups);
        }

        // Delete users in department
        var users = await _context.Users
            .Where(u => u.DepartmentId == depId)
            .ToListAsync();
        _context.Users.RemoveRange(users);

        // Delete batches for department
        var batches = await _context.Batches
            .Where(b => b.DepartmentId == department.DepartmentId)
            .ToListAsync();
        _context.Batches.RemoveRange(batches);

        // Finally delete department
        _context.Departments.Remove(department);

        await _context.SaveChangesAsync();

        return Json(new
        {
            success = true,
            Department = new
            {
                department.DepartmentId,
                department.Name
            },
            message = "Department and all related data deleted successfully"
        });
    }


    //[HttpGet("announcement")]
    //public IActionResult Announcement()
    //{
    //    ViewBag.ActivePage = "Announcement";

    //    var announcements = _context.Announcements
    //        .OrderByDescending(a => a.CreatedAt)
    //        .Select(a => new AnnouncementViewModel
    //        {
    //            AnnouncementId = a.AnnouncementId,
    //            Title = a.Title,
    //            Content = a.Content,
    //            CreatedAt = (DateTime)a.CreatedAt,
    //            Priority = a.Priority == 1 ? "Low Priority" :
    //                       a.Priority == 2 ? "Medium Priority" :
    //                       a.Priority == 3 ? "High Priority" : "Unknown",
    //            Audience = a.Audience == "all" ? "All" :
    //       a.Audience == "hods" ? "HODs" :
    //       a.Audience == "teachers_all" ? "All Teachers" :
    //       a.Audience == "teachers_department" ? "Teachers (Selected Department)" :
    //       a.Audience == "students_all" ? "All Students" :
    //       a.Audience == "students_department" ? "Students (Selected Department)" :
    //       a.Audience == "students_batch" ? "Students (Specific Batch)" :
    //       a.Audience == "students_department_batch" ? "Students (Department & Batch)" :
    //       "Unknown",
    //        })
    //        .ToList();

    //    ViewBag.Announcements = announcements;
    //    return View();
    //}


    [HttpGet("announcement")]
    public IActionResult Announcement(int page = 1, int pageSize = 5)
    {
        ViewBag.ActivePage = "Announcement";

        var query = _context.Announcements.OrderByDescending(a => a.CreatedAt);

        var totalAnnouncements = query.Count();
        var totalPages = (int)Math.Ceiling(totalAnnouncements / (double)pageSize);

        var announcements = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;

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


    //[HttpGet("add-batch")]
    //public IActionResult AddBatch()
    //{

    //    List<Batch> batchDetails = _context.Batches.Include((Batch u) => u.Department).ToList();
    //    base.ViewBag.BatchDetails = batchDetails;
    //    return View();
    //}

    //[HttpPost]
    //public JsonResult AddBatch(int newBatch)
    //{
    //    if (_userService.GetCurrentUser() == null)
    //    {
    //        return Json(new
    //        {
    //            success = false,
    //            message = "User is not authenticated."
    //        });
    //    }
    //    string pattern = "^20\\d{2}$";
    //    if (_context.Batches.Any((Batch u) => u.BatchYear == newBatch))
    //    {
    //        base.TempData["ErrorMessage"] = "This batch is already exists!";
    //        return Json(new
    //        {
    //            success = false
    //        });
    //    }
    //    if (!Regex.IsMatch(newBatch.ToString(), pattern))
    //    {
    //        base.TempData["ErrorMessage"] = "An error occurred while saving the batch " + newBatch;
    //        return Json(new
    //        {
    //            success = false
    //        });
    //    }
    //    List<Department> departments = _context.Departments.ToList();
    //    User currentUser = _userService.GetCurrentUser();
    //    foreach (Department department in departments)
    //    {
    //        Batch batch = new Batch
    //        {
    //            BatchYear = newBatch,
    //            DepartmentId = department.DepartmentId,
    //            StudentsCount = 0
    //        };
    //        _context.Batches.Add(batch);
    //        GCTConnect.Models.Group studentsOnlyGroup = new GCTConnect.Models.Group
    //        {
    //            GroupName = $"{newBatch}_{department.Name}_StudentsOnly",
    //            Description = $"A group only for students batch: {newBatch} & department: {department.Name}",
    //            CreatedBy = currentUser.UserId,
    //            DepartmentId = department.DepartmentId,
    //            CreatedAt = DateTime.UtcNow
    //        };
    //        GCTConnect.Models.Group studentsTeachersHodGroup = new GCTConnect.Models.Group
    //        {
    //            GroupName = $"{newBatch}_{department.Name}_StudentsTeachersHod",
    //            Description = $"A group only for students, teachers, and Hod of batch: {newBatch} & department: {department.Name}",
    //            CreatedBy = currentUser.UserId,
    //            DepartmentId = department.DepartmentId,
    //            CreatedAt = DateTime.UtcNow
    //        };
    //        _context.Groups.AddRange(studentsOnlyGroup, studentsTeachersHodGroup);
    //    }
    //    _context.SaveChanges();
    //    base.TempData["Message"] = newBatch + " Batch has been saved successfully!";
    //    return Json(new
    //    {
    //        success = true
    //    });
    //}


    //public async Task<IActionResult> Dashboard()
    //{
    //    var usersCount = await _context.Users.CountAsync();
    //    ViewBag.UserCount = usersCount;
    //    return View();
    //}

}
