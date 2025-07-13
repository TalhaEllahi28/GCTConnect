using GCTConnect.Models;
using GCTConnect.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GCTConnect.Controllers
{
    public class GetGroupsAndMessages : Controller
    {
        private readonly GctConnectContext _context;
        private readonly IUserService _userService;
        public GetGroupsAndMessages(GctConnectContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }


        [HttpGet]
        public async Task<IActionResult> GetUserGroups()
        {
            int userId = 0;
            if (_userService.GetCurrentUser() != null)
            {
                userId = _userService.GetCurrentUser().UserId;
            }
            // Your method to get the current user's ID
            if (userId == 0)
            {
                return RedirectToAction("Login", "Home");

            }
            var groups = await _context.GroupMembers
                .Where(gm => gm.UserId == userId)
                .Select(gm => new
                {
                    gm.Group.GroupId,
                    gm.Group.GroupName
                })
                .ToListAsync();

            return Json(groups);
        }

        [HttpGet]
        public async Task<IActionResult> GetGroupMessages(int groupId)
        {
            var currentUserName = User.Identity?.Name;

            var messages = await _context.Messages
                .Where(m => m.GroupId == groupId)
                .OrderBy(m => m.Timestamp)
                .Select(m => new
                {
                    m.Content,
                    m.Timestamp,
                    Sender = m.Sender.Username,
                    ProfilePic = m.Sender.ProfilePic,
                    IsCurrentUser = m.Sender.Username == currentUserName,
                    // Add file and audio fields
                    m.FileUrl,
                    m.FileType,
                    m.FileName,
                    m.AudioUrl
                })
                .ToListAsync();

            return PartialView("_ChatWindow", messages);
        }


        [HttpGet]
        public async Task<IActionResult> GetFriends()
        {
            int userId = 0;
            var currentUser = _userService.GetCurrentUser();
            if (currentUser != null)
            {
                userId = currentUser.UserId;
            }

            if (userId == 0)
            {
                return RedirectToAction("Login", "Home");
            }

            var friends = await _context.Friends
                .Where(f => (f.SenderId == userId || f.ReceiverId == userId) && f.Status == "Accepted")
                .Select(f => new
                {
                    FriendId = f.SenderId == userId ? f.ReceiverId : f.SenderId,
                })
                .ToListAsync();

            var friendIds = friends.Select(f => f.FriendId).ToList();

            var friendUsers = await _context.Users
                .Where(u => friendIds.Contains(u.UserId))
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.Name,
                    u.LastName,
                    u.ProfilePic
                })
                .ToListAsync();

            return Json(friendUsers);
        }
        [HttpGet]
        public async Task<IActionResult> GetFriendsMessages(int friendId)
        {
            var currentUserName = User.Identity?.Name;
            var currentUserId = _userService.GetCurrentUser().UserId;
            var messages = await _context.Messages
                .Where(m => (m.ReceiverId == friendId && m.SenderId == currentUserId) || (m.SenderId == friendId && m.ReceiverId == currentUserId))
                .OrderBy(m => m.Timestamp)
                .Select(m => new
                {
                    m.Content,
                    m.Timestamp,
                    Sender = m.Sender.Username,
                    ProfilePic = m.Sender.ProfilePic,
                    IsCurrentUser = m.Sender.Username == currentUserName,
                    // Add file and audio fields
                    m.FileUrl,
                    m.FileType,
                    m.FileName,
                    m.AudioUrl
                })
                .ToListAsync();

            return PartialView("_ChatWindow", messages);
        }

        // Add file upload endpoint
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "No file selected" });
            }

            try
            {
                // Create uploads directory if it doesn't exist
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return file details
                return Json(new
                {
                    success = true,
                    fileUrl = $"/uploads/{fileName}",
                    fileType = file.ContentType,
                    fileName = file.FileName
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Add audio upload endpoint
        [HttpPost]
        public async Task<IActionResult> UploadAudio([FromBody] AudioUploadModel model)
        {
            if (string.IsNullOrEmpty(model.AudioData))
            {
                return Json(new { success = false, message = "No audio data" });
            }

            try
            {
                // Create uploads directory if it doesn't exist
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "audio");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}.wav";
                var filePath = Path.Combine(uploadsDir, fileName);

                // Remove the data URL prefix if present
                var base64Data = model.AudioData;
                if (base64Data.Contains(","))
                {
                    base64Data = base64Data.Split(',')[1];
                }

                // Convert base64 to bytes and save
                var audioBytes = Convert.FromBase64String(base64Data);
                System.IO.File.WriteAllBytes(filePath, audioBytes);

                // Return audio details
                return Json(new
                {
                    success = true,
                    audioUrl = $"/uploads/audio/{fileName}"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetDocuments(int groupId)
        {
            try
            {
                var group = _context.Groups.FirstOrDefault(g => g.GroupId == groupId);

                if (group == null)
                    return Ok(new { success = false, message = "Please Provide GroupId" });

                if (group.CourseId == 0)
                    return Ok(new { success = false, message = "This Group has no CourseId" });

                var documents = _context.Files
                    .Where(f => f.CourseId == group.CourseId && f.FileType == "Document")
                    .ToList();

                var pastPapers = _context.Files
                    .Where(f => f.CourseId == group.CourseId && f.FileType == "PastPapers")
                    .ToList();

                return Ok(new
                {
                    success = true,
                    documents,
                    pastPapers
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Server error: " + ex.Message });
            }
        }

    }

    public class AudioUploadModel
    {
        public string AudioData { get; set; }
    }
}