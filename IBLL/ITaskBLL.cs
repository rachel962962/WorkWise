using System.Collections.Generic;
using DTO;

namespace IBLL
{
    public interface ITaskBLL
    {
        Task AddNewTaskAsync(TaskDTO task);
        Task DeleteTaskAsync(int id);
        Task<List<TaskDTO>> GetAllTasksAsync();
        Task<TaskDTO> GetTaskByIdAsync(int id);
        Task<List<SkillDTO>>  GetRequiredSkillsByTaskIdAsync(int taskId);
        Task <List<TaskDTO>> GetDependenciesByTaskIdAsync(int taskId);
        Task<List<TaskDTO>> GetAllAssignedTasksAsync();
        Task<List<TaskDTO>> GetAllUnassignedTasksAsync();
        Task<List<TaskDTO>> GetAllCompletedTasksAsync();
        Task<List<TaskDTO>> GetAllInProgressTasksAsync();
        Task<List<TaskDTO>> GetAllCancelledTasksAsync();
    }
}
