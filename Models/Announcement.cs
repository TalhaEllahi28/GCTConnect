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

    public virtual Department? Department { get; set; }
}
