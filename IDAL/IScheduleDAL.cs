using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBentities.Models;

namespace IDAL
{
    public interface IScheduleDAL
    {
        Task<List<Schedule>> GetScheduleByDateAsync(DateTime date);
        Task<List<Schedule>> GetScheduleByDateAndTeamAsync(DateTime date, int teamId);
        Task<List<Schedule>> GetAllUncompletedScheduleAsync();
        Task AddNewSchedules(List<Schedule> schedules);
        Task UpdateScheduleStatusAsync(int scheduleId, string status);
        Task<int> GetCancelledTasksCountForTodayByTeamAsync(int teamId);
        Task<int> GetAssignScheduleForTodayByTeamAsync(int teamId);
        Task<int> GetAllOngoingSchedulesCountByTeamAndDateAsync(int teamId);
    }
}
