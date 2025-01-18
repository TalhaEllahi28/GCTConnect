using System;
using System.Collections.Generic;

namespace GCTConnect.Models;

public partial class UserProfile
{
    public int UserId { get; set; }

    public string? Bio { get; set; }

    public string? SocialLinks { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
