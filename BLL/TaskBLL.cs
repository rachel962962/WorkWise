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
        private readonly ITeamDAL teamDal;
        private readonly ISkillDAL skillDal;
        private readonly IMapper mapper;

        public TaskBLL(ITask_Dal taskDal, ITeamDAL teamDAL, ISkillDAL skillDAL)
        {
            this.taskDal = taskDal;
            this.teamDal = teamDAL;
            this.skillDal = skillDAL;
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


        public async Task UpdateTaskAsync(TaskDTO task)
        {
            Task_ task_ = mapper.Map<Task_>(task);
            await taskDal.UpdateTaskAsync(task_);
        }

        public async Task<TaskResponseDto> CreateTaskAsync(TaskCreationDto taskDto)
        {
            // Validate team exists if specified
            if (taskDto.AssignedTeamId.HasValue)
            {
                var teamExists = await teamDal.TeamExistsAsync(taskDto.AssignedTeamId.Value);
                if (!teamExists)
                {
                    throw new ArgumentException($"Team with ID {taskDto.AssignedTeamId.Value} does not exist");
                }
            }

            // Validate skills exist
            if (taskDto.RequiredSkillIds != null)
            {
                foreach (var skillId in taskDto.RequiredSkillIds)
                {
                    var skillExists = await skillDal.SkillExistsAsync(skillId);
                    if (!skillExists)
                    {
                        throw new ArgumentException($"Skill with ID {skillId} does not exist");
                    }
                }
            }

            // Validate dependent tasks exist
            if (taskDto.DependentTaskIds != null)
            {
                foreach (var dependentTaskId in taskDto.DependentTaskIds)
                {
                    var taskExists = await taskDal.TaskExistsAsync(dependentTaskId);
                    if (!taskExists)
                    {
                        throw new ArgumentException($"Dependent task with ID {dependentTaskId} does not exist");
                    }
                }
            }

            // Create task
            var task = new Task_
            {
                Name = taskDto.Name,
                AssignedTeamId = taskDto.AssignedTeamId,
                PriorityLevel = taskDto.PriorityLevel,
                Deadline = taskDto.Deadline,
                Duration = taskDto.Duration,
                RequiredWorkers = taskDto.RequiredWorkers > 0 ? taskDto.RequiredWorkers : 1,
                ComplexityLevel = taskDto.ComplexityLevel
            };

            // Create required skills
            var requiredSkills = new List<TaskRequiredSkill>();
            if (taskDto.RequiredSkillIds != null)
            {
                foreach (var skillId in taskDto.RequiredSkillIds)
                {
                    requiredSkills.Add(new TaskRequiredSkill
                    {
                        SkillId = skillId
                    });
                }
            }

            // Create dependencies
            var dependencies = new List<TaskDependency>();
            if (taskDto.DependentTaskIds != null)
            {
                foreach (var dependentTaskId in taskDto.DependentTaskIds)
                {
                    dependencies.Add(new TaskDependency
                    {
                        DependentTaskId = dependentTaskId
                    });
                }
            }

            // Save task with all related entities
            var createdTask = await taskDal.CreateTaskAsync(task, requiredSkills, dependencies);

            // Map to response DTO
            return await GetTaskByIdAsync(createdTask.TaskId);
        }

        public async Task<TaskResponseDto> GetTaskByIdAsync(int taskId)
        {
            var task = await taskDal.GetTaskByIdAsync(taskId);
            if (task == null)
            {
                throw new ArgumentException($"Task with ID {taskId} does not exist");
            }

            return new TaskResponseDto
            {
                TaskId = task.TaskId,
                Name = task.Name,
                AssignedTeamId = task.AssignedTeamId,
                TeamName = task.AssignedTeam?.Name,
                PriorityLevel = task.PriorityLevel,
                Deadline = task.Deadline,
                Duration = task.Duration,
                RequiredWorkers = task.RequiredWorkers,
                ComplexityLevel = task.ComplexityLevel,
                RequiredSkills = task.TaskRequiredSkills?.Select(rs => new TaskRequiredSkillDTO
                {
                    SkillId = rs.SkillId,
                    SkillName = rs.Skill.Name
                }).ToList() ?? new List<TaskRequiredSkillDTO>(),
                DependentTaskIds = task.TaskDependenciesAsDependent?.Select(d => d.DependentTaskId).ToList() ?? new List<int>()
            };
        }

        public async Task<List<TaskDependencyDTO>> GetAllDependenciesByTasksIdsAsync(List<int> tasksIds)
        {
            var list = await taskDal.GetAllDependenciesByTasksIdsAsync(tasksIds);
            return mapper.Map<List<TaskDependencyDTO>>(list);
        }
    }
}