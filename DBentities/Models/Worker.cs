using System;
using System.Collections.Generic;

namespace DBentities.Models;

public partial class Worker
{
    public int WorkerId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public int? TeamId { get; set; }

    public decimal DailyHours { get; set; }

    public decimal? MaxWeeklyHours { get; set; }

    public int? UserId { get; set; }

    public virtual User? User { get; set; }


    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual Team? Team { get; set; }

    public virtual ICollection<WorkerAbsence> WorkerAbsences { get; set; } = new List<WorkerAbsence>();

    public virtual ICollection<WorkerAvailability> WorkerAvailabilities { get; set; } = new List<WorkerAvailability>();

    public virtual ICollection<WorkerSkill> WorkerSkills { get; set; } = new List<WorkerSkill>();
}
