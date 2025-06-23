using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace BLL
{
    /// <summary>
    /// Class responsible for optimizing assignments to handle split tasks
    /// </summary>
    public class AssignmentOptimizer
    {
        /// <summary>
        /// Optimizes task assignments by consolidating split tasks to the worker who was assigned the most hours
        /// </summary>
        public List<ScheduleDTO> OptimizeAssignments(List<ScheduleDTO> initialSchedule, List<TaskDTO> tasks, List<WorkerDTO> workers)
        {
            // Group schedule entries by TaskId
            var taskGroups = initialSchedule.GroupBy(s => s.TaskId).ToList();
            List<ScheduleDTO> finalSchedule = new List<ScheduleDTO>();

            foreach (var taskGroup in taskGroups)
            {
                int taskId = taskGroup.Key;
                var taskEntries = taskGroup.ToList();

                // If task is assigned to only one worker, no optimization needed
                if (taskEntries.Count == 1)
                {
                    // Update with correct duration and finish time
                    var task = tasks.FirstOrDefault(t => t.TaskId == taskId);
                    var worker = workers.FirstOrDefault(w => w.WorkerId == taskEntries[0].WorkerId);

                    if (task != null && worker != null)
                    {
                        taskEntries[0].AssignedHours = task.Duration;
                        taskEntries[0].FinishTime = CalculateFinishTime(
                            taskEntries[0].StartTime,
                            task.Duration,
                            worker.DailyHours
                        );
                    }

                    finalSchedule.Add(taskEntries[0]);
                }
                else
                {
                    // Task was split among multiple workers, assign to the one with the most hours
                    var workerHours = taskEntries.GroupBy(e => e.WorkerId)
                        .Select(g => new { WorkerId = g.Key, TotalHours = g.Count() })
                        .OrderByDescending(w => w.TotalHours)
                        .ToList();

                    int bestWorkerId = workerHours.First().WorkerId;

                    // Find the task and worker objects
                    var task = tasks.FirstOrDefault(t => t.TaskId == taskId);
                    var worker = workers.FirstOrDefault(w => w.WorkerId == bestWorkerId);

                    if (task != null && worker != null)
                    {
                        // Create a new consolidated schedule entry
                        ScheduleDTO consolidatedEntry = new ScheduleDTO
                        {
                            TaskId = taskId,
                            WorkerId = bestWorkerId,
                            StartTime = taskEntries.Min(e => e.StartTime),
                            AssignedHours = task.Duration,
                            Status = ScheduleStatus.הוקצה.ToString()
                        };

                        // Calculate the finish time based on worker's daily hours
                        consolidatedEntry.FinishTime = CalculateFinishTime(
                            consolidatedEntry.StartTime,
                            task.Duration,
                            worker.DailyHours
                        );

                        finalSchedule.Add(consolidatedEntry);
                    }
                }
            }

            return finalSchedule;
        }

        /// <summary>
        /// Calculates when a task will be finished based on start time, task duration, and worker's daily hours
        /// </summary>
        private DateTime CalculateFinishTime(DateTime startTime, decimal taskDuration, decimal dailyHours)
        {
            // Calculate how many work days the task will take
            int fullWorkDays = (int)(taskDuration / dailyHours);
            decimal remainingHours = taskDuration % dailyHours;

            // Add full work days
            DateTime finishTime = startTime.AddDays(fullWorkDays);

            // Add remaining hours
            if (remainingHours > 0)
            {
                finishTime = finishTime.AddHours((double)remainingHours);
            }

            return finishTime;
        }
    }
}