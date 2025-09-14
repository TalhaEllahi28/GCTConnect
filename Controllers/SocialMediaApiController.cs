using GCTConnect.Models.DTOs;
using GCTConnect.Services;
using Microsoft.AspNetCore.Mvc;

namespace GCTConnect.Controllers.Api
{
    [ApiController]
    [Route("api/social")]
    public class SocialMediaApiController : ControllerBase
    {
        private readonly ISocialMediaService _socialMediaService;
        private readonly ILogger<SocialMediaApiController> _logger;
        private readonly IUserService _userService;
        public SocialMediaApiController(ISocialMediaService socialMediaService, ILogger<SocialMediaApiController> logger, IUserService userService)
        {
            _socialMediaService = socialMediaService;
            _logger = logger;
            _userService = userService;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            try
            {

                var user = _userService.GetCurrentUser();
                if (user == null)
                    return Unauthorized(new { message = "User not authenticated" });

                var userId = user.UserId;
                var profile = await _socialMediaService.GetUserProfileAsync(userId);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, new { message = "Error retrieving profile" });
            }
        }

        [HttpGet("suggested-friends")]
        public async Task<ActionResult<List<SuggestedFriendDto>>> GetSuggestedFriends([FromQuery] string filter = "batch")
        {
            try
            {
                var user = _userService.GetCurrentUser();
                if (user == null)
                    return Unauthorized(new { message = "User not authenticated" });

                var userId = user.UserId;
                var friends = await _socialMediaService.GetSuggestedFriendsAsync(userId, filter);
                return Ok(friends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting suggested friends");
                return StatusCode(500, new { message = "Error retrieving suggested friends" });
            }
        }

        [HttpGet("feed")]
        public async Task<ActionResult<List<PostDto>>> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var user = _userService.GetCurrentUser();
                if (user == null)
                    return Unauthorized(new { message = "User not authenticated" });

                var userId = user.UserId;
                var posts = await _socialMediaService.GetFeedPostsAsync(userId, page, pageSize);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting feed posts");
                return StatusCode(500, new { message = "Error retrieving feed" });
            }
        }

        [HttpPost("posts")]
        public async Task<ActionResult<PostDto>> CreatePost([FromBody] PostCreateDto postDto)
        {
            try
            {
                var user = _userService.GetCurrentUser();
                if (user == null)
                    return Unauthorized(new { message = "User not authenticated" });

                var userId = user.UserId;
                var post = await _socialMediaService.CreatePostAsync(userId, postDto);
                return Ok(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                return StatusCode(500, new { message = "Error creating post" });
            }
        }

        [HttpPost("posts/{postId}/like")]
        public async Task<ActionResult> LikePost(int postId)
        {
            try
            {
                var user = _userService.GetCurrentUser();
                if (user == null)
                    return Unauthorized(new { message = "User not authenticated" });

                var userId = user.UserId;
                var result = await _socialMediaService.LikePostAsync(userId, postId);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error liking post");
                return StatusCode(500, new { message = "Error liking post" });
            }
        }

        [HttpDelete("posts/{postId}/like")]
        public async Task<ActionResult> UnlikePost(int postId)
        {
            try
            {
                var user = _userService.GetCurrentUser();
                if (user == null)
                    return Unauthorized(new { message = "User not authenticated" });

                var userId = user.UserId;
                var result = await _socialMediaService.UnlikePostAsync(userId, postId);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unliking post");
                return StatusCode(500, new { message = "Error unliking post" });
            }
        }

        [HttpGet("trending")]
        public async Task<ActionResult<List<TrendingItemDto>>> GetTrending()
        {
            try
            {
                var user = _userService.GetCurrentUser();
                if (user == null)
                    return Unauthorized(new { message = "User not authenticated" });

                var userId = user.UserId;
                // You can get user's department from session/JWT
                var trending = await _socialMediaService.GetTrendingItemsAsync();
                return Ok(trending);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trending items");
                return StatusCode(500, new { message = "Error retrieving trending items" });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<SearchResultDto>> Search([FromQuery] string q)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(q))
                    return BadRequest(new { message = "Search query is required" });

                var user = _userService.GetCurrentUser();
                if (user == null)
                    return Unauthorized(new { message = "User not authenticated" });

                var userId = user.UserId;
                var results = await _socialMediaService.SearchAsync(q, userId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching");
                return StatusCode(500, new { message = "Error performing search" });
            }
        }

        [HttpPost("friend-request")]
        public async Task<ActionResult> SendFriendRequest([FromBody] int receiverId)
        {
            try
            {
                var user = _userService.GetCurrentUser();
                if (user == null)
                    return Unauthorized(new { message = "User not authenticated" });

                var senderId = user.UserId;
                var result = await _socialMediaService.SendFriendRequestAsync(senderId, receiverId);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending friend request");
                return StatusCode(500, new { message = "Error sending friend request" });
            }
        }

    }
}