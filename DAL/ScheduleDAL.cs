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
    }
}
