using System;
using System.Collections.Generic;

namespace GCTConnect.Models;

public partial class Group
{
    public int GroupId { get; set; }

    public string GroupName { get; set; } = null!;

    public string? Description { get; set; }

    public int CreatedBy { get; set; }

    public int? DepartmentId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CourseId { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual Department? Department { get; set; }

    public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
