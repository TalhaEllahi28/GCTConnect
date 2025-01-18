using System;
using System.Collections.Generic;

namespace GCTConnect.Models;

public partial class ChatbotQuery
{
    public int QueryId { get; set; }

    public int? UserId { get; set; }

    public string? QueryText { get; set; }

    public string? Response { get; set; }

    public DateTime? Timestamp { get; set; }

    public virtual User? User { get; set; }
}
