using System;
using System.Collections.Generic;

namespace GCTConnect.Models;

public partial class Announcement
{
    public int AnnouncementId { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public int? DepartmentId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? Priority { get; set; }

    public string? Audience { get; set; }

    public int UserId { get; set; }

    public bool IsRead { get; set; }

    public virtual ICollection<AnnouncementRecipient> AnnouncementRecipients { get; set; } = new List<AnnouncementRecipient>();

    public virtual Department? Department { get; set; }
}
