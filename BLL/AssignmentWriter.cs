using System;
using System.IO;

namespace BLL
{
    /// <summary>
    /// Simple static class for writing Hungarian algorithm assignment results to a file.
    /// </summary>
    public static class AssignmentWriter
    {
        /// <summary>
        /// Writes the assignments to a file.
        /// </summary>
        /// <param name="filePath">Path to the output file.</param>
        /// <param name="assignments">Assignment array from HungarianAlgorithm.FindAssignments.</param>
        public static void WriteAssignments(string filePath, int[] assignments, double[,] taskIds, double[,] workerIds)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (assignments == null)
                throw new ArgumentNullException(nameof(assignments));
            int sumTasksHours = 0, sumWorkerHours = 0;
            for (int i = 0; i < taskIds.GetLength(0); i++)
            {
                sumTasksHours += (int)taskIds[i, 1];
            }
            for (int i = 0; i < workerIds.GetLength(0); i++)
            {
                sumWorkerHours += (int)workerIds[i, 1];
            }
            int[] expandedTaskIds = new int[sumTasksHours];
            int[] expandedWorkerIds = new int[sumWorkerHours];
            int taskRowStart = 0;
            for (int i = 0; i < taskIds.GetLength(0); i++)
            {
                for (int j = 0; j < (int)taskIds[i, 1]; j++)
                {
                    expandedTaskIds[taskRowStart + j] = (int)taskIds[i, 0];
                }
                taskRowStart += (int)taskIds[i, 1];
            }
            int workerRowStart = 0;
            for (int i = 0; i < workerIds.GetLength(0); i++)
            {
                for (int j = 0; j < (int)workerIds[i, 1]; j++)
                {
                    expandedWorkerIds[workerRowStart + j] = (int)workerIds[i, 0];
                }
                workerRowStart += (int)workerIds[i, 1];
            }
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Agent -> Task Assignments:");

                    for (int i = 0; i < assignments.Length; i++)
                    {
                        int taskIndex = assignments[i];
                        int taskId = expandedTaskIds[taskIndex]; 
                        int workerId = expandedWorkerIds[i];

                        if (taskIndex == -1)
                            writer.WriteLine($"Agent {workerId} -> Not assigned");
                        else
                        {
                            writer.WriteLine($"Agent {workerId} -> Task {taskId}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing assignments to file: {ex.Message}");
            }
        }
    }
}