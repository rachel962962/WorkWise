using System;
using System.Collections.Generic;

namespace DBentities.Models;

public partial class WorkerAbsence
{
    public int AbsenceId { get; set; }

    public int WorkerId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string? Reason { get; set; }

    public virtual Worker Worker { get; set; } = null!;
}
