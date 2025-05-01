using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class ScheduleRequestDTO
    {
        public List<WorkerDTO> Workers { get; set; }
        public List<TaskDTO> Tasks { get; set; }
        public DateTime End { get; set; }
    }

}
