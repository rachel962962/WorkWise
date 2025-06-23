using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class TaskMappingInfo
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public int Priority { get; set; }
        public DateTime Deadline { get; set; }
        public int Complexity { get; set; }
        public decimal Duration { get; set; }
    }
}
