using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;
using IBLL;

namespace BLL
{
    public static class AlgorithmManager 
    {
        public static List<ScheduleDTO> ManageAlgorithm(List<TaskDTO> tasks, List<WorkerDTO> workers, DateTime end, ITaskBLL taskBLL,IWorkerBLL workerBLL, IWorkerSkillBLL workerSkillBLL)
        {
            CostCalculator.Initialize(taskBLL, workerBLL, workerSkillBLL);
            var costMatrixTask = CostCalculator.CalculateCostMatrixAsync(workers, tasks, end);
            var costMatrix = costMatrixTask.Result.Item1;
            var tasksIds = costMatrixTask.Result.Item2;
            var workersIds = costMatrixTask.Result.Item3;
            double[,] expandedCostMatrix = ExpandMatrix.ExpandCostMatrix(costMatrix, tasksIds, workersIds); 
            var result = HungarianAlgorithm.FindAssignments(expandedCostMatrix);
            List<ScheduleDTO> scheduleList = new List<ScheduleDTO>();
            return scheduleList;
        }
    }
}
