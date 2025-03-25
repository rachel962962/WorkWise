using System;
using System.Collections.Generic;

namespace DTO
{
    public partial class WorkerDTO
    {
        public int WorkerId { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public int TeamId { get; set; }

        public decimal DailyHours { get; set; }

        public decimal? MaxWeeklyHours { get; set; }

    }
}

