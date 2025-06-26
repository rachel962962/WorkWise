using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;


namespace IBLL
{
    public interface IFinalSchedule
    {
        Task<List<ScheduleDTO>> CreateFinalSchedule(
    Dictionary<int, int> consolidatedAssignments,
    Dictionary<int, TaskMappingInfo> taskMappings,
    Dictionary<int, WorkerMappingInfo> workerMappings,
    List<TaskDependencyDTO> taskDependencies, List<ScheduleDTO> existingSchedules);

        List<ScheduleDTO> CreateScheduleFromConsolidatedAssignments(
            Dictionary<int, int> consolidatedAssignments,
            Dictionary<int, TaskMappingInfo> taskMappings,
            Dictionary<int, WorkerMappingInfo> workerMappings);
        public DateTime AdjustToWorkingHours(
        DateTime proposedTime,
        int workerId,
        List<WorkerAbsenceDTO> workerAbsences,
        List<WorkerAvailabilityDTO> workerAvailabilities, List<ScheduleDTO> schedules);
        public DateTime CalculateFinishTime(
    DateTime startTime,
    decimal durationHours,
    int workerId,
    List<WorkerAvailabilityDTO> workerAvailabilities,
    List<WorkerAbsenceDTO> workerAbsences,
    List<ScheduleDTO> schedules);
    }
}
