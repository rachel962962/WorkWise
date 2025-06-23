using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class WorkerMappingInfo
    {
        public int WorkerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal? MaxWeeklyHours { get; set; }
        public decimal DailyHours { get; set; }
    }
}
