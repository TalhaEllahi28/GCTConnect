namespace GCTConnect.Models
{
    public class SocialPost
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
        public string Privacy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; }
        public string UserProfilePic { get; set; }
        public int Likes { get; set; }
        public int Comments { get; set; }
    }
}
