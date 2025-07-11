﻿using System;
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
        Task<List<ScheduleDTO?>> GetScheduleByDateAsync(DateTime date);
        Task<List<ScheduleDTO>> GetScheduleByDateAndTeamAsync(DateTime date, int teamId);
        Task<List<ScheduleDTO>> GetAllUncompletedScheduleAsync();
        Task<List<ScheduleDTO>> ManualAssignments(List<TaskAssignmentDto> assignments);
        Task UpdateScheduleStatusAsync(int scheduleId, string status);
        Task<int> GetCancelledTasksCountForTodayByTeamAsync(int teamId);
        Task<int> GetAssignScheduleForTodayByTeamAsync(int teamId);
        Task<int> GetAllOngoingSchedulesCountByTeamAndDateAsync(int teamId);

    }
}
