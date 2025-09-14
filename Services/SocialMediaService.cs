using GCTConnect.Models;
using GCTConnect.Models.DTOs;
using GCTConnect.Services;
using Microsoft.EntityFrameworkCore;

namespace GCTConnect.Services
{
    public class SocialMediaService : ISocialMediaService
    {
        private readonly GctConnectContext _context;

        public SocialMediaService(GctConnectContext context)
        {
            _context = context;
        }

        public async Task<UserProfileDto> GetUserProfileAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Departments)
                .Include(u => u.Batches)
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) throw new ArgumentException("User not found");

            var friendsCount = await _context.Friends
                .CountAsync(f => (f.SenderId == userId || f.ReceiverId == userId) && f.Status == "Accepted");

            var postsCount = await _context.Set<Post>()
                .CountAsync(p => p.UserId == userId);

            return new UserProfileDto
            {
                UserId = user.UserId,
                UserName = user.Username,
                FullName = $"{user.Name} {user.LastName}",
                ProfilePic = user.ProfilePic,
                Role = user.Role,
                Department = user.Departments.Where(d => d.DepartmentId == user.DepartmentId).Select(d => d.Name).FirstOrDefault(),
                Batch = user.BatchId?.ToString(),
                FriendsCount = friendsCount,
                PostsCount = postsCount,
                Bio = user.UserProfile?.Bio
            };
        }

        public async Task<List<SuggestedFriendDto>> GetSuggestedFriendsAsync(int userId, string filterType = "batch")
        {
            var currentUser = await _context.Users
                .Include(u => u.Departments)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (currentUser == null) return new List<SuggestedFriendDto>();

            var existingFriends = await _context.Friends
                .Where(f => (f.SenderId == userId || f.ReceiverId == userId))
                .Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId)
                .ToListAsync();

            existingFriends.Add(userId); // Exclude self

            var query = _context.Users
                .Include(u => u.Departments)
                .Include(u => u.Batches)
                .Where(u => !existingFriends.Contains(u.UserId));

            switch (filterType.ToLower())
            {
                case "batch":
                    query = query.Where(u => u.BatchId == currentUser.BatchId && u.BatchId.HasValue);
                    break;
                case "department":
                    query = query.Where(u => u.DepartmentId == currentUser.DepartmentId && u.DepartmentId.HasValue);
                    break;
                case "interests":
                    // For now, we'll use department as a proxy for interests
                    query = query.Where(u => u.DepartmentId == currentUser.DepartmentId && u.DepartmentId.HasValue);
                    break;
            }

            var suggestions = await query
                .Take(5)
                .Select(u => new SuggestedFriendDto
                {
                    UserId = u.UserId,
                    UserName = u.Username,
                    FullName = $"{u.Name} {u.LastName}",
                    ProfilePic = u.ProfilePic,
                    Department = u.Departments.Where(d => d.DepartmentId == u.DepartmentId).Select(d => d.Name).FirstOrDefault(),
                    Batch = u.BatchId.ToString() ?? "",
                    Role = u.Role,
                    SuggestionReason = GetSuggestionReason(filterType),
                    IsFriendRequestSent = false
                })
                .ToListAsync();

            return suggestions;
        }

        public async Task<List<PostDto>> GetFeedPostsAsync(int userId, int page = 1, int pageSize = 10)
        {
            var currentUser = await _context.Users.FindAsync(userId);
            if (currentUser == null) return new List<PostDto>();

            var friendIds = await _context.Friends
                .Where(f => (f.SenderId == userId || f.ReceiverId == userId) && f.Status == "Accepted")
                .Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId)
                .ToListAsync();

            friendIds.Add(userId); // Include own posts

            var posts = await _context.Set<Post>()
                .Include(p => p.User)
                    .ThenInclude(u => u.Departments)
                .Include(p => p.User)
                    .ThenInclude(u => u.Batches)
                .Include(p => p.PostLikes)
                .Include(p => p.PostComments)
                .Where(p =>
                    friendIds.Contains(p.UserId) ||
                    p.Privacy == "public" ||
                    (p.Privacy == "department" && p.User.DepartmentId == currentUser.DepartmentId) ||
                    (p.Privacy == "batch" && p.User.BatchId == currentUser.BatchId))
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PostDto
                {
                    PostId = p.PostId,
                    UserId = p.UserId,
                    UserName = p.User.Username,
                    UserFullName = $"{p.User.Name} {p.User.LastName}",
                    UserProfilePic = p.User.ProfilePic,
                    UserRole = p.User.Role,
                    UserDepartment = p.User.Departments.Where(d => d.DepartmentId == p.User.DepartmentId).Select(d => d.Name).FirstOrDefault(),
                    UserBatch = p.User.Batches.Where(b => b.BatchId == p.User.BatchId).Select(b => b.BatchYear).FirstOrDefault().ToString(),
                    Content = p.Content,
                    MediaUrl = p.MediaUrl,
                    MediaType = p.MediaType,
                    Privacy = p.Privacy,
                    CreatedAt = p.CreatedAt, // Keep the original datetime
                    LikesCount = p.LikesCount,
                    CommentsCount = p.CommentsCount,
                    IsLiked = p.PostLikes.Any(l => l.UserId == userId),
                    // Remove TimeAgo from the projection - we'll calculate it after
                }).OrderByDescending(p=>p.PostId)
                .ToListAsync();

            // Calculate TimeAgo after the data is materialized
            foreach (var post in posts)
            {
                post.TimeAgo = GetTimeAgo(post.CreatedAt);
            }

            return posts;
        }

        public async Task<PostDto> CreatePostAsync(int userId, PostCreateDto postDto)
        {
            var user = await _context.Users
                .Include(u => u.Departments)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) throw new ArgumentException("User not found");

            var post = new Post
            {
                UserId = userId,
                Content = postDto.Content,
                MediaUrl = postDto.MediaUrl,
                MediaType = postDto.MediaType,
                Privacy = postDto.Privacy,
                CreatedAt = DateTime.Now
            };

            _context.Set<Post>().Add(post);
            await _context.SaveChangesAsync();

            return new PostDto
            {
                PostId = post.PostId,
                UserId = post.UserId,
                UserName = user.Username,
                UserFullName = $"{user.Name} {user.LastName}",
                UserProfilePic = user.ProfilePic,
                UserRole = user.Role,
                UserDepartment = user.Departments.Where(d => d.DepartmentId == user.DepartmentId).Select(d => d.Name).FirstOrDefault(),
                UserBatch = user.BatchId?.ToString(),
                Content = post.Content,
                MediaUrl = post.MediaUrl,
                MediaType = post.MediaType,
                Privacy = post.Privacy,
                CreatedAt = post.CreatedAt,
                LikesCount = 0,
                CommentsCount = 0,
                IsLiked = false,
                TimeAgo = "Just now"
            };
        }

        public async Task<bool> LikePostAsync(int userId, int postId)
        {
            var existingLike = await _context.Set<PostLike>()
                .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);

            if (existingLike != null) return false;

            var like = new PostLike
            {
                UserId = userId,
                PostId = postId,
                CreatedAt = DateTime.Now
            };

            _context.Set<PostLike>().Add(like);

            // Update likes count
            var post = await _context.Set<Post>().FindAsync(postId);
            if (post != null)
            {
                post.LikesCount++;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlikePostAsync(int userId, int postId)
        {
            var existingLike = await _context.Set<PostLike>()
                .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);

            if (existingLike == null) return false;

            _context.Set<PostLike>().Remove(existingLike);

            // Update likes count
            var post = await _context.Set<Post>().FindAsync(postId);
            if (post != null && post.LikesCount > 0)
            {
                post.LikesCount--;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<TrendingItemDto>> GetTrendingItemsAsync(int? departmentId = null)
        {
            var query = _context.Announcements.AsQueryable();

            if (departmentId.HasValue)
            {
                query = query.Where(a => a.DepartmentId == departmentId || a.DepartmentId == null);
            }

            var trending = await query
                .OrderByDescending(a => a.Priority)
                .ThenByDescending(a => a.CreatedAt)
                .Take(3)
                .Select(a => new TrendingItemDto
                {
                    Id = a.AnnouncementId,
                    Title = a.Title,
                    Content = a.Content ?? "",
                    Icon = GetAnnouncementIcon(a.Title),
                    Type = "announcement",
                    CreatedAt = a.CreatedAt ?? DateTime.Now
                })
                .ToListAsync();

            return trending;
        }

        public async Task<SearchResultDto> SearchAsync(string query, int userId)
        {
            var searchTerm = query.ToLower();

            var users = await _context.Users
                .Include(u => u.Departments)
                .Where(u => u.UserId != userId &&
                           (u.Name.ToLower().Contains(searchTerm) ||
                            u.LastName.ToLower().Contains(searchTerm) ||
                            u.Username.ToLower().Contains(searchTerm) ||
                            (u.Departments != null && u.Departments.Where(d=>d.DepartmentId == u.DepartmentId).Select(d=>d.Name).FirstOrDefault().ToLower().Contains(searchTerm))))
                .Take(5)
                .Select(u => new UserSearchDto
                {
                    UserId = u.UserId,
                    UserName = u.Username,
                    FullName = $"{u.Name} {u.LastName}",
                    ProfilePic = u.ProfilePic,
                    Department = u.Departments.Where(d=>d.DepartmentId == u.DepartmentId).Select(d=>d.Name).FirstOrDefault(),
                    Batch = u.BatchId.ToString(),
                    Role = u.Role
                })
                .ToListAsync();

            var posts = await _context.Set<Post>()
                .Include(p => p.User)
                .ThenInclude(u => u.Departments)
                .Where(p => p.Content.ToLower().Contains(searchTerm) &&
                           (p.Privacy == "public" || p.UserId == userId))
                .Take(5)
                .Select(p => new PostDto
                {
                    PostId = p.PostId,
                    UserId = p.UserId,
                    UserName = p.User.Username,
                    UserFullName = $"{p.User.Name} {p.User.LastName}",
                    UserProfilePic = p.User.ProfilePic,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt,
                    TimeAgo = GetTimeAgo(p.CreatedAt)
                })
                .ToListAsync();

            return new SearchResultDto
            {
                Users = users,
                Posts = posts
            };
        }

        public async Task<bool> SendFriendRequestAsync(int senderId, int receiverId)
        {
            var existingRequest = await _context.Friends
                .FirstOrDefaultAsync(f =>
                    (f.SenderId == senderId && f.ReceiverId == receiverId) ||
                    (f.SenderId == receiverId && f.ReceiverId == senderId));

            if (existingRequest != null) return false;

            var friendRequest = new Friend
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Status = "Waiting",
                SentDate = DateTime.Now
            };

            _context.Friends.Add(friendRequest);
            await _context.SaveChangesAsync();
            return true;
        }

        private string GetSuggestionReason(string filterType) => filterType.ToLower() switch
        {
            "batch" => "Same batch",
            "department" => "Same department",
            "interests" => "Similar interests",
            _ => "Suggested for you"
        };

        private string GetTimeAgo(DateTime createdAt)
        {
            var timeSpan = DateTime.Now - createdAt;

            if (timeSpan.TotalMinutes < 1) return "Just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays}d ago";

            return createdAt.ToString("MMM dd");
        }

        private string GetAnnouncementIcon(string title)
        {
            var lowerTitle = title.ToLower();

            if (lowerTitle.Contains("exam")) return "🎓";
            if (lowerTitle.Contains("festival") || lowerTitle.Contains("event")) return "🎪";
            if (lowerTitle.Contains("study") || lowerTitle.Contains("group")) return "📚";
            if (lowerTitle.Contains("workshop") || lowerTitle.Contains("tech")) return "💻";
            if (lowerTitle.Contains("career") || lowerTitle.Contains("job")) return "💼";

            return "📢";
        }
    }
}
