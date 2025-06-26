using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace IBLL
{
    public interface IAlgorithmManager
    {
        public Task<List<ScheduleDTO>> ManageAlgorithmAsync(List<TaskDTO> tasks, List<WorkerDTO> workers, DateTime end, List<ScheduleDTO> existingSchedules);
        public Task<List<ScheduleDTO>> ManualAssignments(List<TaskAssignmentDto> assignments);
    }
}
