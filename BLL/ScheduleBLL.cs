using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DBentities.Models;
using DTO;
using IBLL;
using IDAL;

namespace BLL
{
    public class ScheduleBLL : IScheduleBLL
    {
        private readonly IScheduleDAL scheduleDAL;
        private readonly ITaskBLL taskBLL;
        private readonly IWorkerBLL workerBLL;
        private readonly IWorkerSkillBLL workerSkillBLL;
        private readonly IAlgorithmManager algorithmManager;
        private readonly IMapper mapper;

        public ScheduleBLL(IScheduleDAL scheduleDAL, ITaskBLL taskBLL, IWorkerBLL workerBLL, IWorkerSkillBLL workerSkillBLL, IAlgorithmManager algorithmManager)
        {
            this.scheduleDAL = scheduleDAL;
            this.taskBLL = taskBLL;
            this.workerBLL = workerBLL;
            this.workerSkillBLL = workerSkillBLL;
            this.algorithmManager = algorithmManager;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                // ממפה Schedule → ScheduleDTO
                cfg.CreateMap<Schedule, ScheduleDTO>()
                    .ForMember(dest => dest.WorkerId, opt => opt.MapFrom(src => src.Worker.WorkerId))
                    .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.Task.TaskId));

                // ממפה ScheduleDTO → Schedule (בלי ליצור Worker ו־Task!)
                cfg.CreateMap<ScheduleDTO, Schedule>()
                    .ForMember(dest => dest.WorkerId, opt => opt.MapFrom(src => src.WorkerId))
                    .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.TaskId))
                    .ForMember(dest => dest.Worker, opt => opt.Ignore())
                    .ForMember(dest => dest.Task, opt => opt.Ignore());


                cfg.CreateMap<Worker, WorkerDTO>()
                    .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.WorkerSkills))
                    .ReverseMap();

                cfg.CreateMap<Skill, SkillDTO>()
                    .ReverseMap();
                cfg.CreateMap<Task_, TaskDTO>()
    .ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
            this.algorithmManager = algorithmManager;
        }

        public async Task<List<ScheduleDTO>> CreateNewSchedule(List<WorkerDTO> workers, List<TaskDTO> tasks, DateTime end)
        {
            List<ScheduleDTO> existingSchedules = await GetAllUncompletedScheduleAsync();
            List<ScheduleDTO> result = await algorithmManager.ManageAlgorithmAsync(tasks, workers, end, existingSchedules);
            return result;


        }

        public Task<int> GetAllOngoingSchedulesCountByTeamAndDateAsync(int teamId)
        {
            return scheduleDAL.GetAllOngoingSchedulesCountByTeamAndDateAsync(teamId);
        }

        public async Task<List<ScheduleDTO>> GetAllUncompletedScheduleAsync()
        {
            List<Schedule> schedules = await scheduleDAL.GetAllUncompletedScheduleAsync();
            return schedules.Select(s => mapper.Map<ScheduleDTO>(s)).ToList();
        }

        public Task<int> GetAssignScheduleForTodayByTeamAsync(int teamId)
        {
            return scheduleDAL.GetAssignScheduleForTodayByTeamAsync(teamId);
        }

   

        public Task<int> GetCancelledTasksCountForTodayByTeamAsync(int teamId)
        {
            return scheduleDAL.GetCancelledTasksCountForTodayByTeamAsync(teamId);
        }

        public async Task<List<ScheduleDTO>> GetScheduleByDateAndTeamAsync(DateTime date, int teamId)
        {
            List<Schedule> schedules =await scheduleDAL.GetScheduleByDateAndTeamAsync(date, teamId);
            return schedules.Select(s => mapper.Map<ScheduleDTO>(s)).ToList();
        }

        public async Task<List<ScheduleDTO?>> GetScheduleByDateAsync(DateTime date)
        {
            List<Schedule> schedules = await scheduleDAL.GetScheduleByDateAsync(date);
            return schedules.Select(s => mapper.Map<ScheduleDTO?>(s)).ToList();
        }

        public async Task<List<ScheduleDTO>> ManualAssignments(List<TaskAssignmentDto> assignments)
        {
            if (assignments == null || !assignments.Any())
            {
                throw new ArgumentException("Assignments cannot be null or empty.");
            }

            List<ScheduleDTO> schedules = await algorithmManager.ManualAssignments(assignments);
            await scheduleDAL.AddNewSchedules(schedules.Select(s => mapper.Map<Schedule>(s)).ToList());
            return schedules;
        }

        public Task UpdateScheduleStatusAsync(int scheduleId, string status)
        {
            if (!string.IsNullOrEmpty(status))
            {
                return scheduleDAL.UpdateScheduleStatusAsync(scheduleId, status);
            }
            else
            {
                throw new ArgumentException("Status cannot be null or empty.");
            }
        }
    }
}
