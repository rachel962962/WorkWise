using System;
using System.Collections.Generic;

namespace DBentities.Models;

public partial class Team
{
    public int TeamId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Task_> Tasks { get; set; } = new List<Task_>();

    public virtual ICollection<Worker> Workers { get; set; } = new List<Worker>();
}
