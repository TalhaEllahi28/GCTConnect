using System;
using System.Collections.Generic;

namespace GCTConnect.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public int DepartmentId { get; set; }

    public string CourseName { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public int? FileId { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<File> Files { get; set; } = new List<File>();
}
