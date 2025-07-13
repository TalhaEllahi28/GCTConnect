using System;
using System.Collections.Generic;

namespace GCTConnect.Models;

public partial class File
{
    public int FileId { get; set; }

    public string FileName { get; set; } = null!;

    public string FileUrl { get; set; } = null!;

    public string? FileType { get; set; }

    public int UploaderId { get; set; }

    public int? CourseId { get; set; }

    public DateTime UploadedAt { get; set; }

    public virtual Course? Course { get; set; }

    public virtual User Uploader { get; set; } = null!;
}
