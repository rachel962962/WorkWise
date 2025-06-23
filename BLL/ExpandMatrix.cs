using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DTO;
using static BLL.CostCalculator;

namespace BLL
{
    public static class ExpandMatrix
    {
        public class ExpandedMatrixResult
        {
            public double[,] CostMatrix { get; set; }
            public int[] RowToWorkerMapping { get; set; }
            public int[] ColToTaskMapping { get; set; }
            public Dictionary<int, TaskMappingInfo> TaskMappings { get; set; }
            public Dictionary<int, WorkerMappingInfo> WorkerMappings { get; set; }
        }

        public static ExpandedMatrixResult ExpandCostMatrix(double[,] cost, Dictionary<int, TaskMappingInfo> keyValueTasksPairs, Dictionary<int, WorkerMappingInfo> keyValueWorkersPairs)
        {
            // Calculate total hours for tasks and workers
            decimal sumTasksHours = 0, sumWorkerHours = 0;

            foreach (TaskMappingInfo value in keyValueTasksPairs.Values)
            {
                sumTasksHours += value.Duration;
            }

            foreach (WorkerMappingInfo value in keyValueWorkersPairs.Values)
            {
                sumWorkerHours += value.DailyHours;
            }

            // Create expanded matrix - rows=workers, columns=tasks
            double[,] expandedMatrix = new double[(int)sumWorkerHours, (int)sumTasksHours];

            // Create mapping arrays to track which worker/task each row/column belongs to
            int[] rowToWorkerMapping = new int[(int)sumWorkerHours];
            int[] colToTaskMapping = new int[(int)sumTasksHours];

            // Create arrays to store worker and task information in order
            var workerInfos = keyValueWorkersPairs.OrderBy(kvp => kvp.Key).Select(kvp => new { Id = kvp.Key, Availability = kvp.Value.DailyHours }).ToArray();
            var taskInfos = keyValueTasksPairs.OrderBy(kvp => kvp.Key).Select(kvp => new { Id = kvp.Key, Duration = kvp.Value.Duration }).ToArray();

            int workerRowStart = 0;
            for (int i = 0; i < workerInfos.Length; i++)
            {
                int workerRowCount = (int)workerInfos[i].Availability;
                int taskColStart = 0;

                // Fill row mapping for current worker
                for (int row = workerRowStart; row < workerRowStart + workerRowCount; row++)
                {
                    rowToWorkerMapping[row] = workerInfos[i].Id;
                }

                for (int j = 0; j < taskInfos.Length; j++)
                {
                    int taskColCount = (int)taskInfos[j].Duration;

                    // Fill column mapping for current task (only once per task)
                    if (i == 0) // Fill column mapping only once
                    {
                        for (int col = taskColStart; col < taskColStart + taskColCount; col++)
                        {
                            colToTaskMapping[col] = taskInfos[j].Id;
                        }
                    }

                    // Fill the expanded matrix block
                    for (int row = workerRowStart; row < workerRowStart + workerRowCount; row++)
                    {
                        for (int col = taskColStart; col < taskColStart + taskColCount; col++)
                        {
                            expandedMatrix[row, col] = cost[i, j];
                        }
                    }
                    taskColStart += taskColCount;
                }
                workerRowStart += workerRowCount;
            }

            return new ExpandedMatrixResult
            {
                CostMatrix = expandedMatrix,
                RowToWorkerMapping = rowToWorkerMapping,
                ColToTaskMapping = colToTaskMapping,
                TaskMappings = keyValueTasksPairs,
                WorkerMappings = keyValueWorkersPairs
            };
        }

        // Helper methods to get information about specific cells
        public static int GetWorkerIdForRow(ExpandedMatrixResult result, int row)
        {
            return result.RowToWorkerMapping[row];
        }

        public static int GetTaskIdForColumn(ExpandedMatrixResult result, int col)
        {
            return result.ColToTaskMapping[col];
        }

        public static WorkerMappingInfo GetWorkerInfoForRow(ExpandedMatrixResult result, int row)
        {
            int workerId = result.RowToWorkerMapping[row];
            return result.WorkerMappings[workerId];
        }

        public static TaskMappingInfo GetTaskInfoForColumn(ExpandedMatrixResult result, int col)
        {
            int taskId = result.ColToTaskMapping[col];
            return result.TaskMappings[taskId];
        }

        // Method to get full information about a specific cell
        public static (int WorkerId, int TaskId, WorkerMappingInfo WorkerInfo, TaskMappingInfo TaskInfo) GetCellInfo(ExpandedMatrixResult result, int row, int col)
        {
            int workerId = result.RowToWorkerMapping[row];
            int taskId = result.ColToTaskMapping[col];

            return (
                WorkerId: workerId,
                TaskId: taskId,
                WorkerInfo: result.WorkerMappings[workerId],
                TaskInfo: result.TaskMappings[taskId]
            );
        }
    }
}