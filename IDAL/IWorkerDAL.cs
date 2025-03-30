using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBentities.Models;

namespace IDAL
{
    public interface IWorkerDAL
    {
        Task AddNewWorkerAsync(Worker worker);
        Task<List<Worker>> GetWorkersAsync();
        Task UpdateWorkerAsync(Worker worker);
        Task<Worker> GetWorkerByIdAsync(int id);
        Task DeleteWorkerAsync(int id);
        Task<List<Skill>> GetSkillsByWorkerIdAsync(int workerId);  
        Task<List<WorkerAvailability>> GetWokerAvailabilityByIdAsync(int workerId);
        Task<List<Schedule>> GetWokerScheduleByIdAsync(int workerId);
        Task<List<WorkerAbsence>> GetWokerAbsenceByIdAsync(int workerId);
    }
}
