using System.Collections.Generic;
using System.Threading.Tasks;
using DBentities.Models;
using IBLL;
using IDAL;
using AutoMapper;
using DTO;

namespace BLL
{
    public class TaskBLL : ITaskBLL
    {
        private readonly ITask_Dal taskDal;
        private readonly IMapper mapper;

        public TaskBLL(ITask_Dal taskDal)
        {
            this.taskDal = taskDal;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Task_, TaskDTO>().ReverseMap();
                cfg.CreateMap<Skill, SkillDTO>().ReverseMap();
                cfg.CreateMap<TaskRequiredSkill, TaskRequiredSkillDTO>().ReverseMap(); 
            });
            mapper = new Mapper(configTaskConverter);
        }

        public async Task AddNewTaskAsync(TaskDTO task)
        {
            Task_ task_ = mapper.Map<Task_>(task);
            await taskDal.AddNewTaskAsync(task_);
        }

        public async Task DeleteTaskAsync(int id)
        {
            await taskDal.DeleteTaskAsync(id);
        }

        public async Task<List<TaskDTO>> GetAllAssignedTasksAsync()
        {
            var list = await taskDal.GetAllAssignedTasksAsync();
            return mapper.Map<List<TaskDTO>>(list);
        }

        public async Task<List<TaskDTO>> GetAllCancelledTasksAsync()
        {
            var list = await taskDal.GetAllCancelledTasksAsync();
            return mapper.Map<List<TaskDTO>>(list);
        }

        public async Task<List<TaskDTO>> GetAllCompletedTasksAsync()
        {
            var list = await taskDal.GetAllCompletedTasksAsync();
            return mapper.Map<List<TaskDTO>>(list);
        }

        public async Task<List<TaskDTO>> GetAllInProgressTasksAsync()
        {
            var list = await taskDal.GetAllInProgressTasksAsync();
            return mapper.Map<List<TaskDTO>>(list);
        }

        public async Task<List<TaskDTO>> GetAllTasksAsync()
        {
            var list = await taskDal.GetAllTasksAsync();
            return mapper.Map<List<TaskDTO>>(list);
        }

        public async Task<List<TaskDTO>> GetAllUnassignedTasksAsync()
        {
            var list = await taskDal.GetAllUnassignedTasksAsync();
            return mapper.Map<List<TaskDTO>>(list);
        }

        public async Task<List<TaskDTO>> GetDependenciesByTaskIdAsync(int taskId)
        {
            var list = await taskDal.GetDependenciesByTaskIdAsync(taskId);
            return mapper.Map<List<TaskDTO>>(list);
        }

        public async Task<List<SkillDTO>> GetRequiredSkillsByTaskIdAsync(int taskId)
        {
            var list = await taskDal.GetRequiredSkillsByTaskIdAsync(taskId);
            return mapper.Map<List<SkillDTO>>(list);
        }

        public async Task<TaskDTO> GetTaskByIdAsync(int id)
        {
            var task = await taskDal.GetTaskByIdAsync(id);
            return task != null ? mapper.Map<TaskDTO>(task) : null;
        }

        public async Task UpdateTaskAsync(TaskDTO task)
        {
            Task_ task_ = mapper.Map<Task_>(task);
            await taskDal.UpdateTaskAsync(task_);
        }
    }
}