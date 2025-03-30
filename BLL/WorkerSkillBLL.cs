using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DAL;
using DBentities.Models;
using DTO;
using IBLL;
using IDAL;

namespace BLL
{
    public class WorkerSkillBLL : IWorkerSkillBLL
    {

        private readonly IWorkerSkillDAL workerSkillDAL;
        private readonly IMapper mapper;

        public WorkerSkillBLL(IWorkerSkillDAL workerSkillDAL)
        {
            this.workerSkillDAL = workerSkillDAL;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<WorkerSkill, WorkerSkillDTO>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
        }
        public async Task<ProficiencyLevel> GetProficiencyLevelBySkillAndWorkerId(int skillId, int workerId)
        {
            var workerSkill= await workerSkillDAL.GetWorkerSkillByIdAsync(workerId, skillId);
            return workerSkill != null ? mapper.Map<WorkerSkillDTO>(workerSkill).ProficiencyLevel : ProficiencyLevel.None;
        }
    }
}
