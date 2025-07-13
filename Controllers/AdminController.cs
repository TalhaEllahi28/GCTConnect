using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GCTConnect.Models;
using GCTConnect.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GCTConnect.Controllers;

[Authorize(Roles = "Admin")]
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
    [HttpGet]
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

    public IActionResult Dashboard()
    {
        return View();
    }

    public IActionResult Index()
    {
        return View();
    }
}
