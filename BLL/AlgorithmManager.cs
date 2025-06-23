using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DTO;
using IBLL;
using IDAL;
using static  BLL.CostCalculator;
using static BLL.ExpandMatrix;
namespace BLL
{
    public class AlgorithmManager: IAlgorithmManager
    {
        private readonly ITaskBLL taskBLL;
        private readonly IWorkerBLL workerBLL;
        private readonly IWorkerSkillBLL workerSkillBLL;
        private readonly IFinalSchedule finalSchedule;

        public AlgorithmManager(ITaskBLL taskBLL, IWorkerBLL workerBLL, IWorkerSkillBLL workerSkillBLL, IFinalSchedule finalSchedule)
        {
            this.taskBLL = taskBLL;
            this.workerBLL = workerBLL;
            this.workerSkillBLL = workerSkillBLL;
            this.finalSchedule = finalSchedule;
        }
        public async Task<List<ScheduleDTO>> ManageAlgorithmAsync(List<TaskDTO> tasks, List<WorkerDTO> workers, DateTime end, List<ScheduleDTO> existingSchedules)
        {
            List<TaskDependencyDTO> taskDependencies = await taskBLL.GetAllDependenciesByTasksIdsAsync(tasks.Select(t => t.TaskId).ToList());
            // מזהי המשימות שמופיעות ברשימת התלויות בתור כאלה שתלויים בהן
            var dependedTaskIds = taskDependencies.Select(td => td.TaskId).Distinct().ToList();
            // מזהי המשימות הקיימות בפועל
            var existingTaskIds = tasks.Select(t => t.TaskId) .ToHashSet(); 
            // כל משימה שתלויים בה – אבל היא לא נמצאת ברשימת המשימות
            var missingTasks = dependedTaskIds.Where(id => !existingTaskIds.Contains(id)) .ToList();
            // בדיקה האם יש כאלה
            if (missingTasks.Any())
            {
                // לדוגמה – אפשר להדפיס או לזרוק חריגה או לטפל בזה:
                Console.WriteLine("Missing tasks: " + string.Join(", ", missingTasks));
            }

            // Initialize cost calculator
            CostCalculator.Initialize(taskBLL, workerBLL, workerSkillBLL);

            // Calculate cost matrix and get task and worker information
            var result = await CostCalculator.CalculateCostMatrixAsync(workers, tasks, end);
            double[,] costMatrix = result.CostMatrix;
            Dictionary<int, TaskMappingInfo> keyValueTasksPairs = result.TaskMapping;
            Dictionary<int, WorkerMappingInfo> keyValueWorkersPairs = result.WorkerMapping;

            // Expand cost matrix based on task durations and worker hours
            ExpandedMatrixResult expandedCostMatrix = ExpandMatrix.ExpandCostMatrix(costMatrix, keyValueTasksPairs, keyValueWorkersPairs);

            // Run Hungarian algorithm to find optimal assignments
            var algorithmResult = HungarianAlgorithm.FindAssignments(expandedCostMatrix.CostMatrix);

            // Consolidate assignments - assign each task to the worker who got the most hours
            var consolidatedAssignments = ConsolidateTaskAssignments(algorithmResult, expandedCostMatrix);

            // Create schedule entries based on consolidated assignments
            List<ScheduleDTO> final_schedule = await finalSchedule.CreateFinalSchedule(consolidatedAssignments, keyValueTasksPairs, keyValueWorkersPairs, taskDependencies, existingSchedules);

            return final_schedule;
        }

        /// <summary>
        /// Consolidates task assignments by assigning each complete task to the worker who received the most hours for that task
        /// </summary>
        /// <param name="assignments">Original Hungarian algorithm assignments (hour-by-hour)</param>
        /// <param name="expandedMatrix">The expanded matrix with mapping information</param>
        /// <returns>Dictionary mapping each task to its assigned worker</returns>
        private   Dictionary<int, int> ConsolidateTaskAssignments(int[] assignments, ExpandedMatrixResult expandedMatrix)
        {
            // Dictionary to store how many hours each worker got for each task
            // Key: TaskId, Value: Dictionary<WorkerId, HoursAssigned>
            var taskWorkerHours = new Dictionary<int, Dictionary<int, int>>();

            // Count hours assigned to each worker for each task
            for (int row = 0; row < assignments.Length; row++)
            {
                if (assignments[row] != -1) // If this row (worker hour) is assigned
                {
                    int col = assignments[row];
                    int workerId = expandedMatrix.RowToWorkerMapping[row];
                    int taskId = expandedMatrix.ColToTaskMapping[col];

                    // Initialize dictionaries if they don't exist
                    if (!taskWorkerHours.ContainsKey(taskId))
                    {
                        taskWorkerHours[taskId] = new Dictionary<int, int>();
                    }

                    if (!taskWorkerHours[taskId].ContainsKey(workerId))
                    {
                        taskWorkerHours[taskId][workerId] = 0;
                    }

                    // Increment hour count for this worker-task pair
                    taskWorkerHours[taskId][workerId]++;
                }
            }

            // For each task, find the worker who got the most hours and assign the entire task to them
            var consolidatedAssignments = new Dictionary<int, int>(); // TaskId -> WorkerId

            foreach (var taskEntry in taskWorkerHours)
            {
                int taskId = taskEntry.Key;
                var workerHours = taskEntry.Value;

                // Find the worker with maximum hours for this task
                var bestWorker = workerHours.OrderByDescending(wh => wh.Value).First();
                consolidatedAssignments[taskId] = bestWorker.Key;
            }

            return consolidatedAssignments;
        }

    }
}