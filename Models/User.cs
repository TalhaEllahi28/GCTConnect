using System;
using System.Collections.Generic;

namespace GCTConnect.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Role { get; set; } = null!;

    public int? DepartmentId { get; set; }

    public int? BatchId { get; set; }

    public string? ProfilePic { get; set; }

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? RollNumber { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string Name { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Gender { get; set; }

    public virtual ICollection<ChatbotQuery> ChatbotQueries { get; set; } = new List<ChatbotQuery>();

    public virtual ICollection<Department> Departments { get; set; } = new List<Department>();

    public virtual ICollection<File> Files { get; set; } = new List<File>();

    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual UserProfile? UserProfile { get; set; }

    public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();
}
