using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class WorkerCreationDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? TeamId { get; set; }
        public decimal DailyHours { get; set; }
        public decimal? MaxWeeklyHours { get; set; }
        public UserDTO User { get; set; }
        public List<string> AvailableDays { get; set; }
        public List<WorkerSkillDTO> Skills { get; set; }
    }
}
