using System;
using System.Collections.Generic;

namespace GCTConnect.Models;

public partial class Friend
{
    public int FriendRequestId { get; set; }

    public int SenderId { get; set; }

    public int ReceiverId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime SentDate { get; set; }

    public virtual User Receiver { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
