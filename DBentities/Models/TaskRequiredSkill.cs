using System;
using System.Collections.Generic;

namespace DBentities.Models;

public partial class TaskRequiredSkill
{
    public int TaskId { get; set; }
    public int SkillId { get; set; }

    public virtual Task_ Task { get; set; } = null!;
    public virtual Skill Skill { get; set; } = null!;
}