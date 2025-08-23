using System;
using System.Collections.Generic;

namespace GCTConnect.Models;

public partial class AnnouncementRecipient
{
    public int Id { get; set; }

    public int AnnouncementId { get; set; }

    public int UserId { get; set; }

    public bool? IsRead { get; set; }

    public DateTime? SentAt { get; set; }

    public virtual Announcement Announcement { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
