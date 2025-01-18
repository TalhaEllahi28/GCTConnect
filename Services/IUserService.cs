using GCTConnect.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GCTConnect.Services
{
    public interface IUserService
    {
        User? GetCurrentUser();
    }

    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly GctConnectContext _context;

        public UserService(IHttpContextAccessor httpContextAccessor, GctConnectContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public User? GetCurrentUser()
        {
            // Extract claims from the HTTP context
            var claimsIdentity = _httpContextAccessor.HttpContext?.User.Identity as ClaimsIdentity;

            if (claimsIdentity == null || !claimsIdentity.IsAuthenticated)
                return null;

            var username = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
            var email = claimsIdentity.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email))
                return null;

            // Fetch the user from the database
            return _context.Users.AsNoTracking()
                                 .FirstOrDefault(u => u.Username == username && u.Email == email);
        }
    }
}
