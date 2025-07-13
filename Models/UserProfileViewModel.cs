namespace GCTConnect.Models
{
    public class UserProfileViewModel
    {
        public int UserId { get; set; }

        // Read-only
        public string Username { get; set; }
        public string Role { get; set; }
        public string? DepartmentName { get; set; }
        public string? BatchName { get; set; }
        public string? RollNumber { get; set; }

        // Editable fields
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? Subject { get; set; }

        public string? Bio { get; set; }
        public string? SocialLinks { get; set; }

        //public string? ProfilePicFile { get; set; }
        public string? CurrentProfilePicPath { get; set; }
    }

}
