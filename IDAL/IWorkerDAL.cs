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
        Task<Worker> CreateWorkerAsync(Worker worker, List<WorkerAvailability> availabilities, List<WorkerSkill> skills, User user);
        Task<Worker?> GetWorkerByIdAsync(int workerId);
        Task<bool> WorkerExistsAsync(int workerId);
        Task AddNewWorkerAsync(Worker worker);
        Task<List<Worker>> GetWorkersAsync();
        Task UpdateWorkerAsync(Worker worker);
        Task DeleteWorkerAsync(int id);
        Task<List<Skill>> GetSkillsByWorkerIdAsync(int workerId);
        Task<Worker?> GetWorkerByUserIdAsync(int userId);
        Task<List<WorkerAvailability>> GetWokerAvailabilityByIdAsync(int workerId);
        Task<List<Schedule>> GetWokerScheduleByIdAsync(int workerId);
        Task<List<WorkerAbsence>> GetWokerAbsenceByIdAsync(int workerId);
        Task<List<Worker>> GetWorkersByTeamIdAsync(int teamId);
        Task<List<WorkerAbsence>> GetAllWokerAbsenceAsync();
        Task<List<WorkerAvailability>> GetWokerAvailabilityAsync();
    }
}
