using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class WorkerResponseDto
    {
        public int WorkerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? TeamId { get; set; }
        public string TeamName { get; set; }
        public decimal DailyHours { get; set; }
        public decimal? MaxWeeklyHours { get; set; }
        public List<string> AvailableDays { get; set; }
        public List<WorkerSkillDetailDto> Skills { get; set; }
    }
}
