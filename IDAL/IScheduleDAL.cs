using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBentities.Models;

namespace IDAL
{
    public interface IScheduleDAL
    {
        Task<List<Schedule>> GetScheduleByDateAsync(DateTime date);
      //  Task<List<Schedule>> CreateNewSchedule(List<WorkerDTO> workers, List<TaskDTO> tasks, DateTime end);
    }
}
