
namespace GCTConnect.Models.DTOs
{
    public class PostCreateDto
    {
        public string Content { get; set; } = string.Empty;
        public string Privacy { get; set; } = "public";
        public string? MediaUrl { get; set; }
        public string? MediaType { get; set; }
    }

    public class PostDto
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public string? UserProfilePic { get; set; }
        public string UserRole { get; set; } = string.Empty;
        public string? UserDepartment { get; set; }
        public string? UserBatch { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? MediaUrl { get; set; }
        public string? MediaType { get; set; }
        public string Privacy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public bool IsLiked { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
    }

    public class SuggestedFriendDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePic { get; set; }
        public string Department { get; set; } = string.Empty;
        public string Batch { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string SuggestionReason { get; set; } = string.Empty;
        public bool IsFriendRequestSent { get; set; }
    }

    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePic { get; set; }
        public string Role { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Batch { get; set; }
        public int FriendsCount { get; set; }
        public int PostsCount { get; set; }
        public string? Bio { get; set; }
    }

    public class TrendingItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // announcement, event
        public DateTime CreatedAt { get; set; }
    }

    public class SearchResultDto
    {
        public List<UserSearchDto> Users { get; set; } = new();
        public List<PostDto> Posts { get; set; } = new();
    }

    public class UserSearchDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePic { get; set; }
        public string Department { get; set; } = string.Empty;
        public string? Batch { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}

