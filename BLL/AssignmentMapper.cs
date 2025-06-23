using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace BLL
{
    /// <summary>
    /// Class to handle the mapping between expanded matrix indices and original task/worker IDs
    /// </summary>
    public static class AssignmentMapper
    {
        public class MatrixMappings
        {
            public Dictionary<int, int> ExpandedRowToTaskMap { get; set; } = new Dictionary<int, int>();
            public Dictionary<int, int> ExpandedColToWorkerMap { get; set; } = new Dictionary<int, int>();
        }

        /// <summary>
        /// Creates mappings between expanded matrix indices and original task/worker IDs
        /// </summary>
        public static MatrixMappings CreateExpandedMatrixMappings(double[,] tasksIds, double[,] workersIds)
        {
            var mappings = new MatrixMappings();

            // Build mapping from expanded rows to task IDs
            int taskRowStart = 0;
            for (int i = 0; i < tasksIds.GetLength(0); i++)
            {
                int taskRowCount = (int)tasksIds[i, 1];
                for (int row = taskRowStart; row < taskRowStart + taskRowCount; row++)
                {
                    mappings.ExpandedRowToTaskMap[row] = (int)tasksIds[i, 0];
                }
                taskRowStart += taskRowCount;
            }

            // Build mapping from expanded columns to worker IDs
            int workerColStart = 0;
            for (int j = 0; j < workersIds.GetLength(0); j++)
            {
                int workerColCount = (int)workersIds[j, 1];
                for (int col = workerColStart; col < workerColStart + workerColCount; col++)
                {
                    mappings.ExpandedColToWorkerMap[col] = (int)workersIds[j, 0];
                }
                workerColStart += workerColCount;
            }

            return mappings;
        }
    }
}