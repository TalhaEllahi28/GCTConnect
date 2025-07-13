using System;
using System.Collections.Generic;

namespace GCTConnect.Models;

public partial class CollegeDatum
{
    public int Id { get; set; }

    public string Category { get; set; } = null!;

    public string Data { get; set; } = null!;
}
