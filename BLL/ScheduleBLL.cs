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
        private readonly IMapper mapper;

        public ScheduleBLL(IScheduleDAL scheduleDAL, ITaskBLL taskBLL, IWorkerBLL workerBLL, IWorkerSkillBLL workerSkillBLL)
        {
            this.scheduleDAL = scheduleDAL;
            this.taskBLL = taskBLL;
            this.workerBLL = workerBLL;
            this.workerSkillBLL = workerSkillBLL;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Schedule, ScheduleDTO>()
                .ForMember(dest => dest.WorkerId, opt => opt.MapFrom(src => src.Worker.WorkerId))
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.Task.TaskId))
                .ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
            this.taskBLL = taskBLL;
            this.workerBLL = workerBLL;
            this.workerSkillBLL = workerSkillBLL;
        }

        public Task<List<ScheduleDTO>> CreateNewSchedule(List<WorkerDTO> workers, List<TaskDTO> tasks, DateTime end)
        {
            List<ScheduleDTO> result = AlgorithmManager.ManageAlgorithm(tasks, workers, end,taskBLL,workerBLL, workerSkillBLL);
            return Task.FromResult(result);


        }

        public async Task<List<ScheduleDTO?>> GetScheduleByDateAsync(DateTime date)
        {
            List<Schedule> schedules = await scheduleDAL.GetScheduleByDateAsync(date);
            return schedules.Select(s => mapper.Map<ScheduleDTO?>(s)).ToList();
        }
    }
}
