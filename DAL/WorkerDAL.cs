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
                // Fetch and attach existing skills before saving
                if (worker.WorkerSkills != null && worker.WorkerSkills.Any())
                {
                    var skillIds = worker.WorkerSkills.Select(ws => ws.SkillId).ToList();
                    var existingSkills = await ctx.Skills
                        .Where(s => skillIds.Contains(s.SkillId))
                        .ToDictionaryAsync(s => s.SkillId, s => s);

                    // Clear the collection and re-add with proper references
                    var workerSkills = worker.WorkerSkills.ToList();
                    worker.WorkerSkills.Clear();

                    foreach (var ws in workerSkills)
                    {
                        if (existingSkills.TryGetValue(ws.SkillId, out var skill))
                        {
                            worker.WorkerSkills.Add(new WorkerSkill
                            {
                                WorkerId = worker.WorkerId,
                                SkillId = skill.SkillId,
                                ProficiencyLevel = ws.ProficiencyLevel
                            });
                        }
                        // Skip skills that don't exist - or optionally throw an error
                    }
                }

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

        public async Task<List<Skill>> GetSkillsByWorkerIdAsync(int workerId)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.WorkerSkills
                    .Where(ws => ws.WorkerId == workerId)
                    .Select(ws => ws.Skill)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting skills by worker id", ex);
            }
        }

        public async Task<List<WorkerAbsence>> GetWokerAbsenceByIdAsync(int workerId)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.WorkerAbsences
                    .Where(wa => wa.WorkerId == workerId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting worker absence by id", ex);
            }
        }

        public async Task<List<WorkerAvailability>> GetWokerAvailabilityByIdAsync(int workerId)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.WorkerAvailabilities
                    .Where(wa => wa.WorkerId == workerId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting worker availability by id", ex);
            }
        }

        public async Task<List<Schedule>> GetWokerScheduleByIdAsync(int workerId)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Schedules
                    .Where(s => s.WorkerId == workerId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting worker schedule by id", ex);
            }
        }

        //public async Task<Worker> GetWorkerByIdAsync(int id)
        //{
        //    await using var ctx = new WorkWiseDbContext();
        //    try
        //    {
        //        return await ctx.Workers.FindAsync(id);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error getting worker by id", ex);
        //    }
        //}

        public async Task<List<Worker>> GetWorkersAsync()
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Workers
                    .Include(w => w.WorkerSkills)
                        .ThenInclude(ws => ws.Skill)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting all workers", ex);
            }
        }
        public async Task<bool> WorkerExistsAsync(int workerId)
        {
            await using var ctx = new WorkWiseDbContext();
            return await ctx.Workers.AnyAsync(w => w.WorkerId == workerId);
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
        public async Task<Worker> CreateWorkerAsync(Worker worker, List<WorkerAvailability> availabilities, List<WorkerSkill> skills, User user)
        {
            await using var ctx = new WorkWiseDbContext();

            using (var transaction = await ctx.Database.BeginTransactionAsync())
            {
                try
                {
                    // Add user first to get the generated UserId
                    await ctx.Users.AddAsync(user);
                    await ctx.SaveChangesAsync();

                    // Assign UserId to worker
                    worker.UserId = user.UserId;

                    // Add worker
                    await ctx.Workers.AddAsync(worker);
                    await ctx.SaveChangesAsync();

                    // Add availabilities
                    foreach (var availability in availabilities)
                    {
                        availability.WorkerId = worker.WorkerId;
                        await ctx.WorkerAvailabilities.AddAsync(availability);
                    }

                    // Add skills
                    foreach (var skill in skills)
                    {
                        skill.WorkerId = worker.WorkerId;
                        await ctx.WorkerSkills.AddAsync(skill);
                    }

                    await ctx.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return worker;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
        public async Task<Worker?> GetWorkerByUserIdAsync(int userId)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Workers
                    .Include(w => w.Team)
                    .Include(w => w.WorkerAvailabilities)
                    .Include(w => w.WorkerSkills)
                        .ThenInclude(s => s.Skill)
                    .FirstOrDefaultAsync(w => w.UserId == userId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting worker by user id", ex);
            }
        }
        public async Task<Worker?> GetWorkerByIdAsync(int workerId)
        {
            await using var ctx = new WorkWiseDbContext();
            return await ctx.Workers
                .Include(w => w.Team)
                .Include(w => w.WorkerAvailabilities)
                .Include(w => w.WorkerSkills)
                    .ThenInclude(s => s.Skill)
                .FirstOrDefaultAsync(w => w.WorkerId == workerId);
        }

        public async Task<List<Worker>> GetWorkersByTeamIdAsync(int teamId)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Workers
                    .Where(w => w.TeamId == teamId)
                    .Include(w => w.Team)
                    .Include(w => w.WorkerAvailabilities)
                    .Include(w => w.WorkerSkills)
                        .ThenInclude(s => s.Skill)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting workers by team id", ex);
            }
        }

        public async Task<List<WorkerAbsence>> GetAllWokerAbsenceAsync()
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.WorkerAbsences
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting all worker absences", ex);
            }
        }

        public async Task<List<WorkerAvailability>> GetWokerAvailabilityAsync()
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.WorkerAvailabilities
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting all worker availabilities", ex);
            }
        }

        public async Task<int> GetWokerAbsenceCountByTeamAsync(int teamId)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.WorkerAbsences
                    .Where(wa => wa.Worker.TeamId == teamId && wa.StartDate <= DateTime.Today && wa.EndDate >= DateTime.Today)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting worker absence count by team", ex);
            }
        }
    }
}


