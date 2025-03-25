using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBentities.Models;

namespace IDAL
{
    public interface IWorkerSkillDAL
    {
        Task AddNewWorkerSkillAsync(WorkerSkill workerSkill);
        Task DeleteWorkerSkillAsync(int workerId, int skillId);
        Task<List<WorkerSkill>> GetAllWorkerSkillsAsync();
        Task<WorkerSkill?> GetWorkerSkillByIdAsync(int workerId, int skillId);
        Task UpdateWorkerSkillAsync(WorkerSkill workerSkill);
    }
}
