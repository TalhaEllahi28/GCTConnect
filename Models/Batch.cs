using System;
using System.Collections.Generic;

namespace GCTConnect.Models;

public partial class Batch
{
    public int BatchId { get; set; }

    public int DepartmentId { get; set; }

    public int BatchYear { get; set; }

    public int? StudentsCount { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
