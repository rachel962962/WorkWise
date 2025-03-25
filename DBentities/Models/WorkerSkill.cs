using System;
using System.Collections.Generic;

namespace DBentities.Models;

public partial class WorkerSkill
{
    public int WorkerId { get; set; }

    public int SkillId { get; set; }

    public string? ProficiencyLevel { get; set; }

    public virtual Skill Skill { get; set; } = null!;

    public virtual Worker Worker { get; set; } = null!;
}
