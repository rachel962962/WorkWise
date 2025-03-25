using System;
using System.Collections.Generic;

namespace DTO
{
    public enum WorkDay
    {
        ראשון,
        שני,
        שלישי,
        רביעי,
        חמישי,
        שישי,
        שבת
    }
    public partial class WorkerAvailabilityDTO
    {
        public int WorkerId { get; set; }

        public WorkDay WorkDay { get; set; }
    }
}
