using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBentities.Models;
using DTO;
using IBLL;
using  static BLL.CostCalculator;

namespace BLL
{
    public class FinalSchedule:IFinalSchedule
    {
        private readonly IWorkerBLL workerBLL;

        public FinalSchedule(IWorkerBLL workerBLL)
        {
            this.workerBLL = workerBLL ?? throw new ArgumentNullException(nameof(workerBLL));
        }
        public async Task<List<ScheduleDTO>> CreateFinalSchedule(Dictionary<int, int> consolidatedAssignments, Dictionary<int, TaskMappingInfo> taskMappings, Dictionary<int, WorkerMappingInfo> workerMappings, List<TaskDependencyDTO> taskDependencies, List<ScheduleDTO> existingSchedules)
        {
            List<ScheduleDTO> basicSchedule = CreateScheduleFromConsolidatedAssignments(consolidatedAssignments, taskMappings, workerMappings);
            List<ScheduleDTO> enhancedSchedule = await EnhanceScheduleWithTiming(basicSchedule, consolidatedAssignments, taskMappings, workerMappings, taskDependencies, existingSchedules);
            return enhancedSchedule;
        }

        public List<ScheduleDTO> CreateScheduleFromConsolidatedAssignments(Dictionary<int, int> consolidatedAssignments, Dictionary<int, TaskMappingInfo> taskMappings, Dictionary<int, WorkerMappingInfo> workerMappings)
        {
            var schedule = new List<ScheduleDTO>();
            foreach (var assignment in consolidatedAssignments)
            {
                int taskIndex = assignment.Key;
                int workerIndex = assignment.Value;
                // פרטי עובד ומשימה
                var taskInfo = taskMappings[taskIndex];
                var workerInfo = workerMappings[workerIndex];
                // יצירת רשומת לו"ז בסיסית
                var scheduleEntry = new ScheduleDTO
                {
                    TaskId = taskInfo.TaskId,
                    WorkerId = workerInfo.WorkerId,
                };
                schedule.Add(scheduleEntry);
            }
            return schedule;
        }


        //הוספת שעות התחלה וסיום ללוח הזמנים הבסיסי
        private async Task<List<ScheduleDTO>> EnhanceScheduleWithTiming(List<ScheduleDTO> basicSchedule, Dictionary<int, int> consolidatedAssignments,
         Dictionary<int, TaskMappingInfo> taskMappings, Dictionary<int, WorkerMappingInfo> workerMappings, List<TaskDependencyDTO> taskDependencies, List<ScheduleDTO> schedules)
        {
            var enhancedSchedule = new List<ScheduleDTO>();
            List<WorkerAbsenceDTO> workerAbsences = await workerBLL.GetAllWokerAbsenceAsync();
            List<WorkerAvailabilityDTO> workerAvailabilities = await workerBLL.GetWokerAvailabilityAsync();
            // שלב 1: יצירת גרף תלות בין המשימות
            var dependencyGraph = BuildDependencyGraph(taskMappings.Keys, taskDependencies);
            // שלב 2: מיון טופולוגי של המשימות
            var topologicalOrder = TopologicalSort(dependencyGraph);
            // מעקב אחר זמני סיום של משימות וזמינות עובדים
            var taskFinishTimes = new Dictionary<int, DateTime>();
            var workerScheduledTasks = new Dictionary<int, List<(DateTime StartTime, DateTime EndTime)>>();
            // אתחול זמני זמינות עובדים לזמן התחלה דפולטיבי
            var now = DateTime.Now;
            var todayAtEight = DateTime.Today.AddHours(8);
            var todayAtFive = DateTime.Today.AddHours(17);
            DateTime defaultStartTime;
            if (now < todayAtEight)
            {
                defaultStartTime = todayAtEight;
            }
            else if (now < todayAtFive)
            {
                defaultStartTime = now.AddMinutes(30);
            }
            else
            {
                defaultStartTime = DateTime.Today.AddDays(1).AddHours(8);
            }
            foreach (var workerMapping in workerMappings.Values)
            {
                workerScheduledTasks[workerMapping.WorkerId] = new List<(DateTime, DateTime)>();
            }
            // שלב 3 ו-4: חישוב זמני התחלה והסיום לפי הסדר הטופולוגי
            foreach (var taskIndex in topologicalOrder)
            {
                if (!consolidatedAssignments.ContainsKey(taskIndex))
                    continue;
                var workerIndex = consolidatedAssignments[taskIndex];
                var taskInfo = taskMappings[taskIndex];
                var workerInfo = workerMappings[workerIndex];
                // מציאת הרשומה הבסיסית בלו"ז
                var basicEntry = basicSchedule.FirstOrDefault(s => s.TaskId == taskInfo.TaskId && s.WorkerId == workerInfo.WorkerId);
                // חישוב זמן התחלה המוקדם ביותר עבור המשימה
                var earliestStartTime = CalculateEarliestStartTime(taskIndex, dependencyGraph, taskFinishTimes, workerInfo.WorkerId, workerScheduledTasks, defaultStartTime, workerAbsences, workerAvailabilities, schedules);
                // Update the method call to include the missing 'workerAbsences' parameter
                var finishTime = CalculateFinishTime(earliestStartTime, taskInfo.Duration, workerInfo.WorkerId, workerAvailabilities, workerAbsences, schedules);

                // יצירת רשומת לו"ז משופרת
                var enhancedEntry = new ScheduleDTO
                {
                    TaskId = taskInfo.TaskId,
                    WorkerId = workerInfo.WorkerId,
                    StartTime = earliestStartTime,
                    FinishTime = finishTime,
                    AssignedHours = taskInfo.Duration,
                    Status = "הוקצה"
                };

                // אם יש מידע נוסף ברשומה הבסיסית, נשמר אותו
                if (basicEntry != null)
                {
                    enhancedEntry.Status = basicEntry.Status ?? "הוקצה";
                    // ניתן להעתיק כאן עוד שדות מהרשומה הבסיסית אם נדרש
                }

                enhancedSchedule.Add(enhancedEntry);

                // עדכון זמני מעקב
                taskFinishTimes[taskIndex] = finishTime;
                workerScheduledTasks[workerInfo.WorkerId].Add((earliestStartTime, finishTime));
            }

            return enhancedSchedule;
        }
        // בניית גרף תלות בין המשימות
        private   Dictionary<int, List<int>> BuildDependencyGraph(IEnumerable<int> taskIndices, List<TaskDependencyDTO> dependencies)
        {
            var graph = new Dictionary<int, List<int>>();

            // אתחול הגרף עם כל המשימות
            foreach (var taskIndex in taskIndices)
            {
                graph[taskIndex] = new List<int>();
            }

            // הוספת תלויות לגרף
            if (dependencies != null)
            {
                foreach (var dependency in dependencies)
                {
                    // משימה DependentTaskId תלויה במשימה TaskId
                    if (graph.ContainsKey(dependency.TaskId))
                    {
                        graph[dependency.TaskId].Add(dependency.DependentTaskId);
                    }
                }
            }

            return graph;
        }

        // מיון טופולוגי של המשימות באמצעות אלגוריתם Kahn
        private   List<int> TopologicalSort(Dictionary<int, List<int>> graph)
        {
            var inDegree = new Dictionary<int, int>();
            var result = new List<int>();
            var queue = new Queue<int>();
            // חישוב דרגת כניסה לכל צומת
            foreach (var node in graph.Keys)
            {
                inDegree[node] = 0;
            }
            foreach (var node in graph.Keys)
            {
                foreach (var neighbor in graph[node])
                {
                    if (inDegree.ContainsKey(neighbor))
                    {
                        inDegree[neighbor]++;
                    }
                }
            }
            // הוספת צמתים עם דרגת כניסה 0 לתור
            foreach (var node in inDegree.Keys)
            {
                if (inDegree[node] == 0)
                {
                    queue.Enqueue(node);
                }
            }
            // עיבוד המיון הטופולוגי
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                result.Add(current);

                foreach (var neighbor in graph[current])
                {
                    if (inDegree.ContainsKey(neighbor))
                    {
                        inDegree[neighbor]--;
                        if (inDegree[neighbor] == 0)
                        {
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }
            // בדיקה אם יש מעגל בגרף
            if (result.Count != graph.Keys.Count)
            {
                throw new InvalidOperationException("נמצא מעגל בגרף התלויות - לא ניתן לבצע מיון טופולוגי");
            }
            return result;
        }

        // חישוב זמן התחלה המוקדם ביותר עבור משימה
        private   DateTime CalculateEarliestStartTime(
            int taskIndex,
            Dictionary<int, List<int>> dependencyGraph,
            Dictionary<int, DateTime> taskFinishTimes,
            int workerId,
            Dictionary<int, List<(DateTime StartTime, DateTime EndTime)>> workerScheduledTasks,
            DateTime defaultStartTime,
            List<WorkerAbsenceDTO> workerAbsences,
            List<WorkerAvailabilityDTO> workerAvailabilities, List<ScheduleDTO> schedules)
        {
            var earliestFromDependencies = defaultStartTime;

            // חישוב זמן התחלה על בסיס תלויות
            var predecessors = GetPredecessors(taskIndex, dependencyGraph);
            if (predecessors.Any())
            {
                earliestFromDependencies = predecessors
                    .Where(pred => taskFinishTimes.ContainsKey(pred))
                    .Select(pred => taskFinishTimes[pred])
                    .DefaultIfEmpty(defaultStartTime)
                    .Max();
            }

            // התאמה לזמני עבודה וימי עבודה
            var adjustedStartTime = AdjustToWorkingHours(earliestFromDependencies, workerId, workerAbsences, workerAvailabilities, schedules);

            return adjustedStartTime;
        }

        // בדיקה אם העובד פנוי בטווח זמן מסוים
        private   bool IsWorkerFreeAtTime(int workerId, DateTime startTime, DateTime endTime, Dictionary<int, List<(DateTime StartTime, DateTime EndTime)>> workerScheduledTasks)
        {
            if (!workerScheduledTasks.ContainsKey(workerId))
                return true;

            return !workerScheduledTasks[workerId].Any(task =>
                startTime < task.EndTime && endTime > task.StartTime);
        }

        // מציאת משימות קודמות (predecessors) למשימה נתונה
        private   List<int> GetPredecessors(int taskIndex, Dictionary<int, List<int>> dependencyGraph)
        {
            var predecessors = new List<int>();

            foreach (var kvp in dependencyGraph)
            {
                if (kvp.Value.Contains(taskIndex))
                {
                    predecessors.Add(kvp.Key);
                }
            }

            return predecessors;
        }

        /// <summary>
        /// חישוב זמן סיום תוך התחשבות בשעות עבודה
        /// </summary>
        private   DateTime CalculateFinishTime(
            DateTime startTime,
            decimal durationHours,
            int workerId,
            List<WorkerAvailabilityDTO> workerAvailabilities,
            List<WorkerAbsenceDTO> workerAbsences,
            List<ScheduleDTO> schedules)
        {
            var currentTime = startTime;
            var remainingHours = durationHours;

            while (remainingHours > 0)
            {
                var workingHoursToday = GetDailyWorkingHours(currentTime, workerId, workerAvailabilities);
                var hoursUntilEndOfDay = CalculateHoursUntilEndOfWorkDay(currentTime);

                var hoursToWork = Math.Min((double)remainingHours, Math.Min(workingHoursToday, hoursUntilEndOfDay));

                if (hoursToWork > 0)
                {
                    currentTime = currentTime.AddHours(hoursToWork);
                    remainingHours -= (decimal)hoursToWork;
                }

                if (remainingHours > 0)
                {
                    // מעבר ליום העבודה הבא
                    currentTime = GetNextWorkingDay(currentTime, workerId, workerAbsences, workerAvailabilities, schedules);
                }
            }

            return currentTime;
        }

        /// <summary>
        /// התאמה לשעות עבודה וימי עבודה
        /// </summary>
        private   DateTime AdjustToWorkingHours(
            DateTime proposedTime,
            int workerId,
            List<WorkerAbsenceDTO> workerAbsences,
            List<WorkerAvailabilityDTO> workerAvailabilities, List<ScheduleDTO> schedules)
        {
            var adjustedTime = proposedTime;

            // בדיקה אם העובד זמין באותו יום
            while (!IsWorkerAvailable(adjustedTime, workerId, workerAbsences, workerAvailabilities, schedules))
            {
                adjustedTime = GetNextWorkingDay(adjustedTime, workerId, workerAbsences, workerAvailabilities, schedules);
            }

            // התאמה לשעות עבודה (8:00-17:00 כדפולט)
            if (adjustedTime.Hour < 8)
            {
                adjustedTime = adjustedTime.Date.AddHours(8);
            }
            else if (adjustedTime.Hour >= 17)
            {
                adjustedTime = GetNextWorkingDay(adjustedTime, workerId, workerAbsences, workerAvailabilities, schedules);
            }

            return adjustedTime;
        }

        /// <summary>
        /// בדיקה אם עובד זמין בתאריך נתון
        /// </summary>
        private   bool IsWorkerAvailable(
            DateTime date,
            int workerId,
            List<WorkerAbsenceDTO> workerAbsences,
            List<WorkerAvailabilityDTO> workerAvailabilities, List<ScheduleDTO> schedules)
        {
            // בדיקת היעדרויות
            if (workerAbsences != null)
            {
                var hasAbsence = workerAbsences.Any(a =>
                    a.WorkerId == workerId &&
                    date.Date >= a.StartDate.Date &&
                    date.Date <= a.EndDate.Date);

                if (hasAbsence) return false;
            }

            // בדיקת ימי עבודה
            if (workerAvailabilities != null)
            {
                var dayOfWeek = ConvertToWorkDay(date.DayOfWeek);
                var isAvailable = workerAvailabilities.Any(a =>
                    a.WorkerId == workerId &&
                    a.WorkDay == dayOfWeek);

                return isAvailable;
            }

            //בדיקת שיבוצים קודמים
            if (schedules != null)
            {
                var isScheduled = schedules.Any(s =>
                    s.WorkerId == workerId &&
                    date >= s.StartTime && date < s.FinishTime
                );
                if (isScheduled) return false;
            }

            // אם אין מידע על זמינות, נניח שהעובד זמין בימי ראשון-חמישי
            return date.DayOfWeek >= DayOfWeek.Sunday && date.DayOfWeek <= DayOfWeek.Thursday;
        }

        /// <summary>
        /// המרה מ-DayOfWeek ל-WorkDay
        /// </summary>
        private   WorkDay ConvertToWorkDay(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Sunday => WorkDay.ראשון,
                DayOfWeek.Monday => WorkDay.שני,
                DayOfWeek.Tuesday => WorkDay.שלישי,
                DayOfWeek.Wednesday => WorkDay.רביעי,
                DayOfWeek.Thursday => WorkDay.חמישי,
                DayOfWeek.Friday => WorkDay.שישי,
                DayOfWeek.Saturday => WorkDay.שבת,
                _ => WorkDay.ראשון
            };
        }

        /// <summary>
        /// מציאת יום העבודה הבא
        /// </summary>
        private   DateTime GetNextWorkingDay(
            DateTime currentDate,
            int workerId,
            List<WorkerAbsenceDTO> workerAbsences,
            List<WorkerAvailabilityDTO> workerAvailabilities, List<ScheduleDTO> schedules)
        {
            var nextDay = currentDate.Date.AddDays(1).AddHours(8); // יום הבא ב-8:00

            // חיפוש יום העבודה הבא
            while (!IsWorkerAvailable(nextDay, workerId, workerAbsences, workerAvailabilities, schedules))
            {
                nextDay = nextDay.AddDays(1);
            }

            return nextDay;
        }

        /// <summary>
        /// קבלת שעות עבודה יומיות
        /// </summary>
        private   double GetDailyWorkingHours(
            DateTime date,
            int workerId,
            List<WorkerAvailabilityDTO> workerAvailabilities)
        {
            // כברירת מחדל - 8 שעות עבודה ביום
            return 8.0;
        }

        /// <summary>
        /// חישוב כמה שעות נותרו עד סוף יום העבודה
        /// </summary>
        private   double CalculateHoursUntilEndOfWorkDay(DateTime currentTime)
        {
            var endOfWorkDay = currentTime.Date.AddHours(17); // 17:00

            if (currentTime >= endOfWorkDay)
                return 0;

            return (endOfWorkDay - currentTime).TotalHours;
        }
    }
}