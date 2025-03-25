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
        Task AddNewWorkerAsync(WorkerDTO worker);
        Task<List<WorkerDTO>> GetWorkersAsync();
        Task UpdateWorkerAsync(WorkerDTO worker);
        Task<WorkerDTO> GetWorkerByIdAsync(int id);
        Task DeleteWorkerAsync(int id);
    }
}
