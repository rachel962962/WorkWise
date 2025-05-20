using System;
using System.Collections.Generic;
using System.Linq;
using DBentities.Models;
using IDAL;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class Task_Dal : ITask_Dal
    {
        public async Task AddNewTaskAsync(Task_ task)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                await ctx.Tasks.AddAsync(task);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new task", ex);
            }
        }

        public async Task DeleteTaskAsync(int id)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                var task = await ctx.Tasks.FindAsync(id);
                if (task != null)
                {
                    ctx.Tasks.Remove(task);
                    await ctx.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Task not found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting task", ex);
            }
        }

        public async Task<List<Task_>> GetAllAssignedTasksAsync()
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Tasks
                    .Where(t => ctx.Schedules
                        .Any(s => s.TaskId == t.TaskId && s.Status == "הוקצה"))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting all in-progress tasks", ex);
            }
        }

        public async Task<List<Task_>> GetAllCancelledTasksAsync()
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Tasks
                    .Where(t => ctx.Schedules
                        .Any(s => s.TaskId == t.TaskId && s.Status == "בוטל"))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting all in-progress tasks", ex);
            }
        }

        public async Task<List<Task_>> GetAllCompletedTasksAsync()
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Tasks
                    .Where(t => ctx.Schedules
                        .Any(s => s.TaskId == t.TaskId && s.Status == "הושלם"))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting all in-progress tasks", ex);
            }
        }

        public async Task<List<Task_>> GetAllInProgressTasksAsync()
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Tasks
                    .Where(t => ctx.Schedules
                        .Any(s => s.TaskId == t.TaskId && s.Status == "בתהליך"))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting all in-progress tasks", ex);
            }
        }

        public async Task<List<Task_>> GetAllTasksAsync()
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Tasks.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting all tasks", ex);
            }
        }

        public async Task<List<Task_>> GetAllUnassignedTasksAsync()
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Tasks
                    .Where(t => !ctx.Schedules
                        .Any(s => s.TaskId == t.TaskId))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting all unassigned tasks", ex);
            }
        }

        public async Task<List<Task_>> GetDependenciesByTaskIdAsync(int taskId)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.TaskDependencies
                    .Where(td => td.TaskId == taskId)
                    .Select(td => td.DependentTask)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting dependencies by task id", ex);
            }
        }

        public async Task<List<Skill>> GetRequiredSkillsByTaskIdAsync(int taskId)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.TaskRequiredSkills
                    .Where(ts => ts.TaskId == taskId)
                    .Select(ts => ts.Skill)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting required skills by task id", ex);
            }
        }

        public async Task<Task_?> GetTaskByIdAsync(int id)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Tasks.FirstOrDefaultAsync(t => t.TaskId == id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting task by id", ex);
            }
        }

        public async Task UpdateTaskAsync(Task_ task)
        {
            await using var ctx = new WorkWiseDbContext();
            var existingTask = await ctx.Tasks.FirstOrDefaultAsync(t => t.TaskId == task.TaskId);
            if (existingTask != null)
            {
                var teamExists = await ctx.Teams.AnyAsync(t => t.TeamId == task.AssignedTeamId);
                if (!teamExists)
                {
                    throw new Exception("Invalid team_id");
                }
                try
                {
                    ctx.Entry(existingTask).CurrentValues.SetValues(task);
                    await ctx.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error updating task", ex);
                }
            }
            else
            {
                throw new Exception("Task not found");
            }
        }
    }
}
