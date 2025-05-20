using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DBentities.Models;
using DTO;
using IBLL;
using IDAL;

namespace BLL
{
    public class WorkerBLL : IWorkerBLL
    {
        private readonly IWorkerDAL workerDAL;
        private readonly IMapper mapper;

        public WorkerBLL(IWorkerDAL workerDAL)
        {
            this.workerDAL = workerDAL;

            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Worker, WorkerDTO>()
                    .ForMember(dest => dest.Skills, opt => opt.MapFrom(src =>
                        src.WorkerSkills.Select(ws => ws.Skill)))
                    .ReverseMap()
                    .ForMember(dest => dest.WorkerSkills, opt => opt.MapFrom(src =>
                        src.Skills.Select(skillDto => new WorkerSkill
                        {
                            SkillId = skillDto.SkillId
                        })));
                cfg.CreateMap<Skill, SkillDTO>().ReverseMap();
                cfg.CreateMap<WorkerAvailability, WorkerAvailabilityDTO>().ReverseMap();
                cfg.CreateMap<WorkerAbsence, WorkerAbsenceDTO>().ReverseMap();

            });

            mapper = new Mapper(configTaskConverter);
        }

        public async Task AddNewWorkerAsync(WorkerDTO worker)
        {
            await workerDAL.AddNewWorkerAsync(mapper.Map<Worker>(worker));
        }

        public async Task DeleteWorkerAsync(int id)
        {
            await workerDAL.DeleteWorkerAsync(id);
        }

        public async Task<List<SkillDTO>> GetSkillsByWorkerIdAsync(int workerId)
        {
            var skills =  await workerDAL.GetSkillsByWorkerIdAsync(workerId);
            return mapper.Map<List<SkillDTO>>(skills);
        }

        public async Task<List<WorkerAbsenceDTO>> GetWokerAbsenceByIdAsync(int workerId)
        {
            var list =await workerDAL.GetWokerAbsenceByIdAsync(workerId);
            return mapper.Map<List<WorkerAbsenceDTO>>(list);
        }

        public async Task<List<WorkerAvailabilityDTO>> GetWokerAvailabilityByIdAsync(int workerId)
        {
            var list =await workerDAL.GetWokerAvailabilityByIdAsync(workerId);
            return mapper.Map<List<WorkerAvailabilityDTO>>(list);
        }

        public async Task<List<ScheduleDTO>> GetWokerScheduleByIdAsync(int workerId)
        {
            var list =await workerDAL.GetWokerScheduleByIdAsync(workerId);
            return mapper.Map<List<ScheduleDTO>>(list);
        }

        public async Task<WorkerDTO> GetWorkerByIdAsync(int id)
        {
            var worker = await workerDAL.GetWorkerByIdAsync(id);
            return worker != null ? mapper.Map<WorkerDTO>(worker) : null;
        }

        public async Task<List<WorkerDTO>> GetWorkersAsync()
        {
            var list = await workerDAL.GetWorkersAsync();
            return mapper.Map<List<WorkerDTO>>(list);
        }

        public async Task UpdateWorkerAsync(WorkerDTO worker)
        {
            Worker worker1 = mapper.Map<Worker>(worker);
            await workerDAL.UpdateWorkerAsync(worker1);
        }
    }
}
