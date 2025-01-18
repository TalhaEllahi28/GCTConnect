using System;
using System.Collections.Generic;

namespace GCTConnect.Models;

public partial class GroupMember
{
    public int GroupMemberId { get; set; }

    public int? GroupId { get; set; }

    public int? UserId { get; set; }

    public string Role { get; set; } = null!;

    public DateTime? JoinedAt { get; set; }

    public virtual Group? Group { get; set; }

    public virtual User? User { get; set; }
}
