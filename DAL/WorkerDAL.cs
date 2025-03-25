using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBentities.Models;
using IDAL;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class WorkerDAL : IWorkerDAL
    {
        public async Task AddNewWorkerAsync(Worker worker)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                await ctx.Workers.AddAsync(worker);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new worker", ex);
            }
        }

        public async Task DeleteWorkerAsync(int id)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                var worker = await ctx.Workers.FindAsync(id);
                if (worker != null)
                {
                    ctx.Workers.Remove(worker);
                    await ctx.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Worker not found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting worker", ex);
            }
        }

        public async Task<Worker> GetWorkerByIdAsync(int id)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Workers.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting worker by id", ex);
            }
        }

        public async Task<List<Worker>> GetWorkersAsync()
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Workers.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting all workers", ex);
            }
        }

        public async Task UpdateWorkerAsync(Worker worker)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                var workerToUpdate = await ctx.Workers.FindAsync(worker.WorkerId);
                if (workerToUpdate != null)
                {
                    var teamExists = await ctx.Teams.AnyAsync(t => t.TeamId == worker.TeamId);
                    if (!teamExists)
                    {
                        throw new Exception("Invalid team_id");
                    }
                    ctx.Entry(workerToUpdate).CurrentValues.SetValues(worker);
                    await ctx.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Worker not found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating worker", ex);
            }
        }
    }
}


