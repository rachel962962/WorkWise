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
        Task UpdateTaskAsync(TaskDTO task);
    }
}
