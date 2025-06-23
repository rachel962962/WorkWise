using System;
using System.Collections.Generic;
using DBentities.Models;

namespace DTO
{
    public enum ComplexityLevel
    {
        נמוך=1,
        בינוני,
        גבוה
    }
    public partial class TaskDTO
    {
        public int TaskId { get; set; }

        public string Name { get; set; } = null!;

        public int AssignedTeamId { get; set; }

        public int PriorityLevel { get; set; }

        public DateTime Deadline { get; set; }

        public decimal Duration { get; set; }

        public int RequiredWorkers { get; set; }

        public ComplexityLevel ComplexityLevel { get; set; }

        List<ScheduleDTO> Schedules { get; set; } = new();
        List<TaskRequiredSkillDTO> TaskRequiredSkills { get; set; } = new List<TaskRequiredSkillDTO>();


    }
}