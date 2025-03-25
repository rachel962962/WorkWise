using System;
using System.Collections.Generic;

namespace DBentities.Models;

public partial class TaskDependency
{
    public int TaskId { get; set; }

    public int DependentTaskId { get; set; }
    public virtual Task_ Task { get; set; } = null!;
    public virtual Task_ DependentTask { get; set; } = null!;
}
