namespace GCTConnect.Models
{
    public class SocialUser
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string ProfilePic { get; set; }
        public string Department { get; set; }
        public string Batch { get; set; }
        public int FriendCount { get; set; }
        public int PostCount { get; set; }
    }
}
