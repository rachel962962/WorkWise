using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBentities.Models;
using IDAL;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class ScheduleDAL : IScheduleDAL
    {
        public async Task AddNewSchedules(List<Schedule> schedules)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                await ctx.Schedules.AddRangeAsync(schedules);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new schedules", ex);
            }
        }

        public async Task<int> GetAllOngoingSchedulesCountByTeamAndDateAsync(int teamId)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Schedules
                    .Where(s => s.StartTime.Date <= DateTime.Today && s.FinishTime.Date >= DateTime.Today && s.Worker.TeamId == teamId && s.Status == "בתהליך")
                    .CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting ongoing schedules count by team and date", ex);
            }
        }

        public async Task<List<Schedule>> GetAllUncompletedScheduleAsync()
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Schedules
                    .Where(s => s.FinishTime > DateTime.Now)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting uncompleted schedules", ex);
            }
        }

        public async Task<int> GetAssignScheduleForTodayByTeamAsync(int teamId)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Schedules
                    .Where(s => s.StartTime.Date <= DateTime.Today && s.FinishTime.Date >= DateTime.Today && s.Worker.TeamId == teamId && s.Status == "הוקצה")
                    .CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting assigned schedules for today by team", ex);
            }
        }

        public async Task<int> GetCancelledTasksCountForTodayByTeamAsync(int teamId)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Schedules
                    .Where(s => s.StartTime.Date <= DateTime.Today && s.FinishTime.Date >= DateTime.Today && s.Worker.TeamId == teamId && s.Status == "בוטל")
                    .CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting cancelled tasks count for today by team", ex);
            }
        }

        public async Task<List<Schedule>> GetScheduleByDateAndTeamAsync(DateTime date, int teamId)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Schedules
     .Where(s => s.StartTime.Date <= date.Date && s.FinishTime.Date >= date.Date && s.Worker.TeamId == teamId)
     .Include(s => s.Task)
     .Include(s => s.Worker)
     .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting schedules by date and team", ex);
            }
        }

        public async Task<List<Schedule>> GetScheduleByDateAsync(DateTime date)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Schedules.Where(s => s.StartTime.Date <= date.Date && s.FinishTime.Date >= date.Date)
                    .Include(s => s.Task)
                    .Include(s => s.Worker)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting schedules", ex);
            }
        }

        public async Task UpdateScheduleStatusAsync(int scheduleId, string status)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                var schedule = await ctx.Schedules.FindAsync(scheduleId);
                if (schedule == null)
                {
                    throw new Exception("Schedule not found");
                }
                schedule.Status = status;
                ctx.Entry(schedule).State = EntityState.Modified;
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating schedule status", ex);
            }
        }
    }
}
