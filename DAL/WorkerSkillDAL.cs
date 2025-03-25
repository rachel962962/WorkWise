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
    public class WorkerSkillDAL : IWorkerSkillDAL
    {
        public async Task AddNewWorkerSkillAsync(WorkerSkill workerSkill)
        {
           await using var ctx= new WorkWiseDbContext();
            try
            {
                await ctx.WorkerSkills.AddAsync(workerSkill);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new worker skill", ex);
            }
        }

        public async Task DeleteWorkerSkillAsync(int workerId, int skillId)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                var workerSkill = await ctx.WorkerSkills.FindAsync(workerId, skillId);
                if (workerSkill != null)
                {
                    ctx.WorkerSkills.Remove(workerSkill);
                    await ctx.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Worker skill not found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting worker skill", ex);
            }

        }

        public async Task<List<WorkerSkill>> GetAllWorkerSkillsAsync()
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.WorkerSkills.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting all worker skills", ex);
            }
        }

        public async Task<WorkerSkill?> GetWorkerSkillByIdAsync(int workerId, int skillId)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.WorkerSkills.FindAsync(workerId, skillId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting worker skill by id", ex);
            }
        }

        public async Task UpdateWorkerSkillAsync(WorkerSkill workerSkill)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                var workerSkillToUpdate = await ctx.WorkerSkills.FindAsync(workerSkill.SkillId, workerSkill.WorkerId);
                if (workerSkillToUpdate != null)
                {
                    ctx.Entry(workerSkillToUpdate).CurrentValues.SetValues(workerSkill);
                    await ctx.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Worker skill not found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating worker skill", ex);
            }
        }
    }
}
