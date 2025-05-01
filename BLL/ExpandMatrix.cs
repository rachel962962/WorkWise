using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DTO;

namespace BLL
{
    public static class ExpandMatrix
    {
        public static double[,] ExpandCostMatrix(double[,] cost, double[,] tasksIds, double[,] workersIds)
        {
            double sumTasksHours = 0, sumWorkerHours = 0;
            for (int i = 0; i < tasksIds.GetLength(0); i++)
            {
                sumTasksHours += tasksIds[i, 1];
            }
            for (int i = 0; i < workersIds.GetLength(0); i++)
            {
                sumWorkerHours += workersIds[i, 1];
            }
            double[,] expandedMatrix = new double[(int)sumTasksHours, (int)sumWorkerHours];
            int taskRowStart = 0;

            for (int i = 0; i < tasksIds.GetLength(0); i++)
            {
                int taskRowCount = (int)tasksIds[i, 1];
                int workerColStart = 0;

                for (int j = 0; j < workersIds.GetLength(0); j++)
                {
                    int workerColCount = (int)workersIds[j, 1];

                    for (int row = taskRowStart; row < taskRowStart + taskRowCount; row++)
                    {
                        for (int col = workerColStart; col < workerColStart + workerColCount; col++)
                        {
                            expandedMatrix[row, col] = cost[i, j];
                        }
                    }

                    workerColStart += workerColCount;
                }

                taskRowStart += taskRowCount;
            }

            return expandedMatrix;
        }
    }
}
