using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class TaskCreationDto
    {
        public string Name { get; set; }
        public int? AssignedTeamId { get; set; }
        public int? PriorityLevel { get; set; }
        public DateTime? Deadline { get; set; }
        public decimal Duration { get; set; }
        public int RequiredWorkers { get; set; }
        public string ComplexityLevel { get; set; }
        public List<int> RequiredSkillIds { get; set; }
        public List<int> DependentTaskIds { get; set; }
    }
}
