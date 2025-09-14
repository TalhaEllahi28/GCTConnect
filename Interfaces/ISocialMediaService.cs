using GCTConnect.Models.DTOs;

namespace GCTConnect.Services
{
    public interface ISocialMediaService
    {
        Task<UserProfileDto> GetUserProfileAsync(int userId);
        Task<List<SuggestedFriendDto>> GetSuggestedFriendsAsync(int userId, string filterType = "batch");
        Task<List<PostDto>> GetFeedPostsAsync(int userId, int page = 1, int pageSize = 10);
        Task<PostDto> CreatePostAsync(int userId, PostCreateDto postDto);
        Task<bool> LikePostAsync(int userId, int postId);
        Task<bool> UnlikePostAsync(int userId, int postId);
        Task<List<TrendingItemDto>> GetTrendingItemsAsync(int? departmentId = null);
        Task<SearchResultDto> SearchAsync(string query, int userId);
        Task<bool> SendFriendRequestAsync(int senderId, int receiverId);
    }
}