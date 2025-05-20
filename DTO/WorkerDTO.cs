using System;
using System.Collections.Generic;
using DBentities.Models;

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
        public List<SkillDTO> Skills { get; set; } = new();
        List<ScheduleDTO> Schedules { get; set; } = new();


    }
}

