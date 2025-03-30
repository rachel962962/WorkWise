using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace IBLL
{
    public interface IWorkerSkillBLL
    {
        Task<ProficiencyLevel> GetProficiencyLevelBySkillAndWorkerId(int skillId , int workerId);
    }
}
