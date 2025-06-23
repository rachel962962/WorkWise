using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBentities.Models;
using DTO;

namespace IBLL
{
    public interface IWorkerBLL
    {
        Task<WorkerResponseDto> CreateWorkerAsync(WorkerCreationDto workerDto);
        Task<WorkerResponseDto> GetWorkerByIdAsync(int workerId);
        Task AddNewWorkerAsync(WorkerDTO worker);
        Task<List<WorkerDTO>> GetWorkersAsync();
        Task UpdateWorkerAsync(WorkerDTO worker);
        Task DeleteWorkerAsync(int id);
        Task<List<SkillDTO>> GetSkillsByWorkerIdAsync(int workerId);
        Task<List<WorkerAvailabilityDTO>> GetWokerAvailabilityByIdAsync(int workerId);
        Task<List<WorkerAvailabilityDTO>> GetWokerAvailabilityAsync();
        Task<List<ScheduleDTO>> GetWokerScheduleByIdAsync(int workerId);
        Task <List<WorkerAbsenceDTO>> GetWokerAbsenceByIdAsync(int workerId);
        Task <List<WorkerAbsenceDTO>> GetAllWokerAbsenceAsync();
        Task<WorkerResponseDto?> GetWorkerByUserIdAsync(int userId);
        Task<List<WorkerDTO>> GetWorkersByTeamIdAsync(int teamId);

    }
}
