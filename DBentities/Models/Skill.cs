using System;
using System.Collections.Generic;

namespace DBentities.Models;

public partial class Skill
{
    public int SkillId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<WorkerSkill> WorkerSkills { get; set; } = new List<WorkerSkill>();
    public virtual ICollection<TaskRequiredSkill> TaskRequiredSkills { get; set; } = new List<TaskRequiredSkill>();
}
