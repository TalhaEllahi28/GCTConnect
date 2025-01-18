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
            if (userId==0) 
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
                    IsCurrentUser = m.Sender.Username == currentUserName // Check if the sender is the current user
                })
                .ToListAsync();

            return PartialView("_ChatWindow", messages);
        }



    }
}




