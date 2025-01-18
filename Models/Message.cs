using System;
using System.Collections.Generic;

namespace GCTConnect.Models;

public partial class Message
{
    public int MessageId { get; set; }

    public int GroupId { get; set; }

    public int SenderId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? Timestamp { get; set; }

    public bool? IsCurrentUser { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
