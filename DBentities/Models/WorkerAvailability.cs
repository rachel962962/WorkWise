using System;
using System.Collections.Generic;

namespace DBentities.Models;

public partial class WorkerAvailability
{
    public int WorkerId { get; set; }

    public string WorkDay { get; set; } = null!;

    public virtual Worker Worker { get; set; } = null!;
}
