using System;
using System.Collections.Generic;

namespace DBentities.Models;

public partial class Schedule
{
    public int ScheduleId { get; set; }

    public int TaskId { get; set; }

    public int WorkerId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime FinishTime { get; set; }

    public decimal AssignedHours { get; set; }

    public string? Status { get; set; }

    public virtual Task_ Task { get; set; } = null!;

    public virtual Worker Worker { get; set; } = null!;
}
