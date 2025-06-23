using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBentities.Models;

namespace DTO
{
    public enum ScheduleStatus
    {
        הוקצה,
        בתהליך,
        הושלם,
        בוטל
    }
    public class ScheduleDTO
    {
        public int ScheduleId { get; set; }

        public int TaskId { get; set; }

        public int WorkerId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime FinishTime { get; set; }

        public decimal AssignedHours { get; set; }

        public string? Status { get; set; }
        public virtual TaskDTO Task { get; set; } = null!;

        public virtual WorkerDTO Worker { get; set; } = null!;

    }
}
