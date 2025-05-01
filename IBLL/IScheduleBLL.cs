using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace IBLL
{
    public interface IScheduleBLL
    {
        Task<List<ScheduleDTO>> CreateNewSchedule(List<WorkerDTO> workers, List<TaskDTO> tasks, DateTime end);

    }
}
