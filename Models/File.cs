using System;
using System.Collections.Generic;

namespace GCTConnect.Models;

public partial class File
{
    public int FileId { get; set; }

    public int GroupId { get; set; }

    public int SenderId { get; set; }

    public string FileName { get; set; } = null!;

    public string? FileType { get; set; }

    public string FilePath { get; set; } = null!;

    public DateTime? UploadedAt { get; set; }

    public virtual Group Group { get; set; } = null!;

    public virtual User Sender { get; set; } = null!;
}
