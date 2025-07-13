using System;
using System.Collections.Generic;

namespace GCTConnect.Models;

public partial class Message
{
    public int MessageId { get; set; }

    public int? GroupId { get; set; }

    public int SenderId { get; set; }

    public int? ReceiverId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime? Timestamp { get; set; }

    public string? FileUrl { get; set; }

    public string? FileType { get; set; }

    public string? FileName { get; set; }

    public string? AudioUrl { get; set; }

    public virtual Group? Group { get; set; }

    public virtual User Sender { get; set; } = null!;
}
