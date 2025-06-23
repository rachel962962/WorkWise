using System.Collections.Generic;
using DBentities.Models;

namespace IDAL
{
    public interface ITask_Dal
    {
        Task<Task_> CreateTaskAsync(Task_ task, List<TaskRequiredSkill> requiredSkills, List<TaskDependency> dependencies);
        Task<Task_?> GetTaskByIdAsync(int taskId);
        Task<bool> TaskExistsAsync(int taskId);
        Task AddNewTaskAsync(Task_ task);
        Task<List<Task_>> GetAllTasksAsync();
        Task UpdateTaskAsync(Task_ task);
        Task DeleteTaskAsync(int id);
        Task<List<Skill>> GetRequiredSkillsByTaskIdAsync(int taskId);
        Task<List<Task_>> GetDependenciesByTaskIdAsync(int taskId);
        Task<List<Task_>> GetAllAssignedTasksAsync();
        Task<List<Task_>> GetAllUnassignedTasksAsync();
        Task<List<Task_>> GetAllCompletedTasksAsync();
        Task<List<Task_>> GetAllInProgressTasksAsync();
        Task<List<Task_>> GetAllCancelledTasksAsync();
        Task<List<TaskDependency>> GetAllDependenciesByTasksIdsAsync(List<int> tasksIds);

    }
}

