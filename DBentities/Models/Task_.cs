using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DBentities.Models;

public partial class Task_
{
    public int TaskId { get; set; }
    public string Name { get; set; } = null!;
    public int? AssignedTeamId { get; set; }
    public int? PriorityLevel { get; set; }
    public DateTime? Deadline { get; set; }
    public decimal Duration { get; set; }
    public int? RequiredWorkers { get; set; } = 1;
    public string? ComplexityLevel { get; set; } = "Medium";

    public virtual Team? AssignedTeam { get; set; }
    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual ICollection<TaskDependency> TaskDependenciesAsParent { get; set; } = new List<TaskDependency>();
    public virtual ICollection<TaskDependency> TaskDependenciesAsDependent { get; set; } = new List<TaskDependency>();

    public virtual ICollection<TaskRequiredSkill> TaskRequiredSkills { get; set; } = new List<TaskRequiredSkill>();
}