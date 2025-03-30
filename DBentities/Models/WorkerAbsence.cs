using System;
using System.Collections.Generic;

namespace DBentities.Models;

public partial class WorkerAbsence
{
    public int AbsenceId { get; set; }

    public int WorkerId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? Reason { get; set; }

    public virtual Worker Worker { get; set; } = null!;
}
