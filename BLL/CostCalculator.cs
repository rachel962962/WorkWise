using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBentities.Models;
using DTO;
using IBLL;

namespace BLL
{
    public static class CostCalculator
    {
        private static ITaskBLL ITaskBLL;
        private static IWorkerBLL IWorkerBLL;
        private static IWorkerSkillBLL IWorkerSkillBLL;


        public static void Initialize(ITaskBLL taskBLL, IWorkerBLL workerBLL, IWorkerSkillBLL workerSkillBLL)
        {
            ITaskBLL = taskBLL;
            IWorkerBLL = workerBLL;
            IWorkerSkillBLL = workerSkillBLL;
        }

        // חישוב מטריצת העלויות עבור הקצאות עובד-משימה
        public static async Task<CostMatrixResult> CalculateCostMatrixAsync(List<WorkerDTO> workers, List<TaskDTO> tasks, DateTime end)
        {
            if (workers.Count == 0)
            {
                throw new ArgumentException("No workers available");
            }
            if (tasks.Count == 0)
            {
                throw new ArgumentException("No tasks available");
            }
            if (end < DateTime.Today)
            {
                throw new ArgumentException("End date is over");
            }

            // יצירת דיקשנריז מיפוי מקומיים
            Dictionary<int, TaskMappingInfo> taskMapping = new Dictionary<int, TaskMappingInfo>();
            Dictionary<int, WorkerMappingInfo> workerMapping = new Dictionary<int, WorkerMappingInfo>();

            // מילוי דיקשנרי המיפוי של המשימות
            for (int t = 0; t < tasks.Count; t++)
            {
                taskMapping[t] = new TaskMappingInfo
                {
                    TaskId = tasks[t].TaskId,
                    TaskName = tasks[t].Name,
                    Priority = (int)tasks[t].PriorityLevel,
                    Deadline = tasks[t].Deadline,
                    Duration = tasks[t].Duration
                };
            }

            // מילוי דיקשנרי המיפוי של העובדים
            for (int w = 0; w < workers.Count; w++)
            {
                workerMapping[w] = new WorkerMappingInfo
                {
                    WorkerId = workers[w].WorkerId,
                    FirstName = workers[w].FirstName,
                    LastName = workers[w].LastName,
                    MaxWeeklyHours = workers[w].MaxWeeklyHours,
                    DailyHours = workers[w].DailyHours
                };
            }

            double[,,] tempCostMatrix = new double[tasks.Count, workers.Count, 6];

            for (int t = 0; t < tasks.Count; t++)
            {
                for (int w = 0; w < workers.Count; w++)
                {
                    try
                    {
                        tempCostMatrix[t, w, 0] = await CalculateDeadLine(tasks[t]);
                        tempCostMatrix[t, w, 1] = await CalculateSkillMatching(tasks[t], workers[w]);
                        tempCostMatrix[t, w, 2] = await CalculateDependencies(tasks[t]);
                        tempCostMatrix[t, w, 3] = await CalculatePriority(tasks[t]);
                        tempCostMatrix[t, w, 4] = await CalculateExperience(tasks[t], workers[w]);
                        tempCostMatrix[t, w, 5] = await CalculateAvailability(tasks[t], workers[w], end, w, workerMapping);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("", ex);
                    }
                }
            }

            //  מטריצה עם עובדים בשורות ומשימות בעמודות 
            double[,] costMatrix = new double[workers.Count, tasks.Count];

            var normalizationFactors = await CalculateNormalizationFactorsAsync(tempCostMatrix);

            Factors factors = CalculateFactors(tempCostMatrix);

            //  שינוי סדר הלולאות: עובדים קודם, משימות שנית
            for (int w = 0; w < workers.Count; w++)
            {
                for (int t = 0; t < tasks.Count; t++)
                {
                    try
                    {
                        costMatrix[w, t] = await CalculateCostForTaskWorkerPairAsync(ExtractSlice(tempCostMatrix, t, w), factors);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Error calculating final cost for Worker {w}, Task {t}", ex);
                    }
                }
            }

            return new CostMatrixResult
            {
                CostMatrix = costMatrix,
                TaskMapping = taskMapping,
                WorkerMapping = workerMapping
            };
        }


        // חישוב דד-ליין לפני נרמול
        private static async Task<double> CalculateDeadLine(TaskDTO task)
        {
            double days = (task.Deadline - DateTime.Now).TotalDays;
            // Ensure positive value and handle edge cases
            return Math.Max(0.1, days); // Minimum 0.1 days to avoid division by zero
        }

        // חישוב התאמה בין משימה לעובד לפני נרמול
        private static async Task<double> CalculateSkillMatching(TaskDTO task, WorkerDTO worker)
        {
            try
            {
                // Get required skills for the task
                var requiredSkillIds = await ITaskBLL.GetRequiredSkillsByTaskIdAsync(task.TaskId);
                if (requiredSkillIds == null || !requiredSkillIds.Any())
                {
                    return 0.0; // If no specific skills required, any worker can do it
                }

                // Get worker's skills
                var workerSkills = await IWorkerBLL.GetSkillsByWorkerIdAsync(worker.WorkerId);
                if (workerSkills == null || !workerSkills.Any())
                {
                    return 1.0; // Worker has no skills
                }

                // Calculate matching skills
                int matchingSkills = requiredSkillIds.Count(skillId =>
                    workerSkills.Any(ws => ws.SkillId == skillId.SkillId));

                return (double)matchingSkills / requiredSkillIds.Count();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CalculateSkillMatching: {ex.Message}");
                return 0.5; // Default value
            }
        }

        // חישוב תלויות לפני נרמול
        private static async Task<double> CalculateDependencies(TaskDTO task)
        {
            try
            {
                // Get dependencies for the task
                var dependencies = await ITaskBLL.GetDependenciesByTaskIdAsync(task.TaskId);
                return dependencies?.Count() ?? 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CalculateDependencies: {ex.Message}");
                return 0.0;
            }
        }

        // חישוב עדיפות לפני נרמול
        private static async Task<double> CalculatePriority(TaskDTO task)
        {
            return Math.Max(1.0, (double)task.PriorityLevel); // Ensure minimum value of 1
        }

        // חישוב התאמה בין רמת הקושי של המשימה לרמת המומחיות של העובד
        private static async Task<double> CalculateExperience(TaskDTO task, WorkerDTO worker)
        {
            try
            {
                // Get required skills for the task
                var requiredSkillIds = await ITaskBLL.GetRequiredSkillsByTaskIdAsync(task.TaskId);
                if (requiredSkillIds == null || !requiredSkillIds.Any())
                {
                    return 0.0; // Default experience match
                }

                // Get worker's skills with proficiency
                var workerSkills = await IWorkerBLL.GetSkillsByWorkerIdAsync(worker.WorkerId);
                if (workerSkills == null || !workerSkills.Any())
                {
                    return 1.0; // High cost for inexperienced worker
                }

                double sum = 0;
                int complexity = Math.Max(1, (int)task.ComplexityLevel); // Ensure minimum complexity of 1

                foreach (var skill in requiredSkillIds)
                {
                    var matchingWorkerSkill = workerSkills.FirstOrDefault(ws => ws.SkillId == skill.SkillId);
                    if (matchingWorkerSkill != null)
                    {
                        var proficiencyTask = IWorkerSkillBLL.GetProficiencyLevelBySkillAndWorkerId(matchingWorkerSkill.SkillId, worker.WorkerId);
                        ProficiencyLevel proficiency = await proficiencyTask;

                        int proficiencyLevel = GetProficiencyLevel(proficiency);

                        if (proficiencyLevel == complexity - 1)
                        {
                            sum += 0.5;
                        }
                        else if (proficiencyLevel < complexity)
                        {
                            sum += 1.0;
                        }
                        else
                        {
                            sum += 0.0; // Overqualified, good match
                        }
                    }
                    else
                    {
                        sum += 1.0; // Worker doesn't have required skill
                    }
                }

                return sum / requiredSkillIds.Count();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CalculateExperience: {ex.Message}");
                return 0.5; // Default value
            }
        }

        private static int GetProficiencyLevel(ProficiencyLevel proficiency)
        {
            switch (proficiency)
            {
                case ProficiencyLevel.מתחיל:
                    return 1;
                case ProficiencyLevel.בינוני:
                    return 2;
                case ProficiencyLevel.מומחה:
                    return 3;
                default:
                    return 1; // Default to beginner
            }
        }

        // חישוב זמינות
        private static async Task<double> CalculateAvailability(TaskDTO task, WorkerDTO worker, DateTime end, int w, Dictionary<int, WorkerMappingInfo> workerMapping)
        {
            try
            {
                DateTime start = DateTime.Now;
                decimal hours = 0;
                var workerAvailability = await IWorkerBLL.GetWokerAvailabilityByIdAsync(worker.WorkerId);

                if (workerAvailability == null)
                {
                    return 1.0; // High cost for unavailable worker
                }

                List<WorkDay> workDays = workerAvailability.Select(day => day.WorkDay).ToList();

                // Availability based on daily hours and task duration
                while (start < end)
                {
                    WorkDay currentDay = MapDayOfWeekToWorkDay(start.DayOfWeek);

                    if (workDays.Contains(currentDay))
                    {
                        if (!await CheckIfWorkerIsSchedule(worker, start) && !await CheckIfWorkerIsAbsence(worker, start))
                        {
                            hours += worker.DailyHours;
                        }
                    }
                    start = start.AddDays(1);
                }
                if (hours == 0)
                {
                    return 1.0; // Worker completely unavailable
                }

                if (task.Duration == 0)
                {
                    return 0.0; // No duration task, perfect availability
                }

                double ratio = (double)(task.Duration / hours);
                return Math.Min(1.0, ratio); // Cap at 1.0
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CalculateAvailability: {ex.Message}");
                return 0.5; // Default availability
            }
        }

        private static double[,,] Normalize(double[,,] mat, NormalizationFactors factors)
        {
            try
            {
                int deadlineRange = factors.MaxDeadline - factors.MinDeadline;
                if (deadlineRange == 0) deadlineRange = 1; // Avoid division by zero

                for (int t = 0; t < mat.GetLength(0); t++)
                {
                    for (int w = 0; w < mat.GetLength(1); w++)
                    {
                        // Normalize deadline
                        mat[t, w, 0] = (mat[t, w, 0] - factors.MinDeadline) / deadlineRange;

                        // Normalize dependencies
                        if (factors.MaxDependencies > 0)
                        {
                            mat[t, w, 2] = mat[t, w, 2] / factors.MaxDependencies;
                        }

                        // Normalize priority
                        if (factors.MaxPriority > 0)
                        {
                            mat[t, w, 3] = mat[t, w, 3] / factors.MaxPriority;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Normalize: {ex.Message}");
            }

            return mat;
        }

        public static double CalculateDeadlineWeight(double[,,] mat)
        {
            try
            {
                double sum = 0;
                int dim1 = mat.GetLength(0);
                int dim2 = mat.GetLength(1);
                for (int i = 0; i < dim1; i++)
                {
                    for (int j = 0; j < dim2; j++)
                    {
                        double value = 1 - mat[i, j, 0];
                        if (!double.IsNaN(value) && !double.IsInfinity(value))
                        {
                            sum += value;
                        }
                    }
                }
                return sum / dim1;
            }
            catch
            {
                return 0.2;
            }
        }

        public static double CalculateSkillMatchingWeight(double[,,] mat)
        {
            try
            {
                double sum = 0;
                int dim1 = mat.GetLength(0);
                int dim2 = mat.GetLength(1);
                for (int i = 0; i < dim1; i++)
                {
                    for (int j = 0; j < dim2; j++)
                    {
                        if (!double.IsNaN(mat[i, j, 1]) && !double.IsInfinity(mat[i, j, 1]))
                        {
                            sum += mat[i, j, 1];
                        }
                    }
                }
                return 1 - (sum / (dim1 * dim2));
            }
            catch
            {
                return 0.25; // Default weight
            }
        }

        public static double CalculateDependenciesWeight(double[,,] mat)
        {
            try
            {
                double sum = 0;
                int dim1 = mat.GetLength(0);
                int dim2 = mat.GetLength(1);
                for (int i = 0; i < dim1; i++)
                {
                    for (int j = 0; j < dim2; j++)
                    {
                        if (!double.IsNaN(mat[i, j, 2]) && !double.IsInfinity(mat[i, j, 2]))
                        {
                            sum += mat[i, j, 2];
                        }
                    }
                }
                return sum / dim1;
            }
            catch
            {
                return 0.15; // Default weight
            }
        }

        public static double CalculatePriorityWeight(double[,,] mat)
        {
            try
            {
                double sum = 0;
                int dim1 = mat.GetLength(0);
                int dim2 = mat.GetLength(1);
                for (int i = 0; i < dim1; i++)
                {
                    for (int j = 0; j < dim2; j++)
                    {
                        if (!double.IsNaN(mat[i, j, 3]) && !double.IsInfinity(mat[i, j, 3]))
                        {
                            sum += mat[i, j, 3];
                        }
                    }
                }
                return sum / dim1;
            }
            catch
            {
                return 0.15; // Default weight
            }
        }

        public static double CalculateExperienceWeight(double[,,] mat)
        {
            try
            {
                double sum = 0;
                int dim1 = mat.GetLength(0);
                int dim2 = mat.GetLength(1);
                for (int i = 0; i < dim1; i++)
                {
                    for (int j = 0; j < dim2; j++)
                    {
                        if (!double.IsNaN(mat[i, j, 4]) && !double.IsInfinity(mat[i, j, 4]))
                        {
                            sum += mat[i, j, 4];
                        }
                    }
                }
                return sum / (dim1 * dim2);
            }
            catch
            {
                return 0.15; // Default weight
            }
        }

        public static double CalculateAvailabilityWeight(double[,,] mat)
        {
            try
            {
                double sum = 0;
                int dim1 = mat.GetLength(0);
                int dim2 = mat.GetLength(1);
                for (int i = 0; i < dim1; i++)
                {
                    for (int j = 0; j < dim2; j++)
                    {
                        if (!double.IsNaN(mat[i, j, 5]) && !double.IsInfinity(mat[i, j, 5]))
                        {
                            sum += mat[i, j, 5];
                        }
                    }
                }
                return sum / (dim1 * dim2);
            }
            catch
            {
                return 0.1; // Default weight
            }
        }

        private static Factors CalculateFactors(double[,,] mat)
        {
            double deadlineWeight = CalculateDeadlineWeight(mat);
            double skillMatchingWeight = CalculateSkillMatchingWeight(mat);
            double dependenciesWeight = CalculateDependenciesWeight(mat);
            double priorityWeight = CalculatePriorityWeight(mat);
            double experienceWeight = CalculateExperienceWeight(mat);
            double availabilityWeight = CalculateAvailabilityWeight(mat);

            return new Factors
            {
                DeadlineWeight = deadlineWeight,
                SkillMatchingWeight = skillMatchingWeight,
                DependenciesWeight = dependenciesWeight,
                PriorityWeight = priorityWeight,
                ExperienceWeight = experienceWeight,
                AvailabilityWeight = availabilityWeight
            };
        }

        private static async Task<double> CalculateCostForTaskWorkerPairAsync(double[] mat, Factors factors)
        {
            try
            {
                double cost = 0;
                cost += (double.IsNaN(mat[0]) ? 0 : mat[0]) * factors.DeadlineWeight;
                cost += (double.IsNaN(mat[1]) ? 0 : mat[1]) * factors.SkillMatchingWeight;
                cost += (double.IsNaN(mat[2]) ? 0 : mat[2]) * factors.DependenciesWeight;
                cost += (double.IsNaN(mat[3]) ? 0 : mat[3]) * factors.PriorityWeight;
                cost += (double.IsNaN(mat[4]) ? 0 : mat[4]) * factors.ExperienceWeight;
                cost += (double.IsNaN(mat[5]) ? 0 : mat[5]) * factors.AvailabilityWeight;
                return await Task.FromResult(Math.Max(0.001, cost));
            }
            catch
            {
                return 1.0; // Default cost
            }
        }

        private static WorkDay MapDayOfWeekToWorkDay(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return WorkDay.ראשון;
                case DayOfWeek.Monday:
                    return WorkDay.שני;
                case DayOfWeek.Tuesday:
                    return WorkDay.שלישי;
                case DayOfWeek.Wednesday:
                    return WorkDay.רביעי;
                case DayOfWeek.Thursday:
                    return WorkDay.חמישי;
                case DayOfWeek.Friday:
                    return WorkDay.שישי;
                case DayOfWeek.Saturday:
                    return WorkDay.שבת;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dayOfWeek));
            }
        }

        // בדיקה האם העובד מתוכנן ביום נתון
        public static async Task<bool> CheckIfWorkerIsSchedule(WorkerDTO worker, DateTime day)
        {
            try
            {
                var workerSchedule = await IWorkerBLL.GetWokerScheduleByIdAsync(worker.WorkerId);
                if (workerSchedule == null) return false;

                foreach (var item in workerSchedule)
                {
                    if (day >= item.StartTime && day <= item.FinishTime)
                        return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> CheckIfWorkerIsAbsence(WorkerDTO worker, DateTime day)
        {
            try
            {
                var workerAbsence = await IWorkerBLL.GetWokerAbsenceByIdAsync(worker.WorkerId);
                if (workerAbsence == null) return false;

                foreach (var item in workerAbsence)
                {
                    if (day >= item.StartDate && day <= item.EndDate)
                        return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<double> GetMax(double[,,] mat, int z)
        {
            double max = double.MinValue;
            int dim1 = mat.GetLength(0);
            int dim2 = mat.GetLength(1);

            for (int i = 0; i < dim1; i++)
            {
                for (int j = 0; j < dim2; j++)
                {
                    if (!double.IsNaN(mat[i, j, z]) && !double.IsInfinity(mat[i, j, z]))
                    {
                        max = Math.Max(max, mat[i, j, z]);
                    }
                }
            }

            return max == double.MinValue ? 1.0 : max;
        }

        private static async Task<double> GetMin(double[,,] mat, int z)
        {
            double min = double.MaxValue;
            int dim1 = mat.GetLength(0);
            int dim2 = mat.GetLength(1);

            for (int i = 0; i < dim1; i++)
            {
                for (int j = 0; j < dim2; j++)
                {
                    if (!double.IsNaN(mat[i, j, z]) && !double.IsInfinity(mat[i, j, z]))
                    {
                        min = Math.Min(min, mat[i, j, z]);
                    }
                }
            }

            return min == double.MaxValue ? 0.0 : min;
        }

        static double[] ExtractSlice(double[,,] matrix, int x, int y)
        {
            double[] slice = new double[6];

            for (int k = 0; k < 6; k++)
            {
                slice[k] = matrix[x, y, k];
            }

            return slice;
        }


        public class CostMatrixResult
        {
            public double[,] CostMatrix { get; set; }
            public Dictionary<int, TaskMappingInfo> TaskMapping { get; set; }
            public Dictionary<int, WorkerMappingInfo> WorkerMapping { get; set; }
        }



        private static async Task<NormalizationFactors> CalculateNormalizationFactorsAsync(double[,,] mat)
        {
            double maxDeadline = await GetMax(mat, 0);
            double maxDependencies = await GetMax(mat, 2);
            double maxPriority = await GetMax(mat, 3);
            double minDeadline = await GetMin(mat, 0);

            return new NormalizationFactors
            {
                MinDeadline = (int)minDeadline,
                MaxDeadline = (int)Math.Max(minDeadline + 1, maxDeadline),
                MaxDependencies = (int)Math.Max(1, maxDependencies),
                MaxPriority = (int)Math.Max(1, maxPriority)
            };
        }
    }

    internal class NormalizationFactors
    {
        public int MinDeadline { get; set; }
        public int MaxDeadline { get; set; }
        public int MaxDependencies { get; set; }
        public int MaxPriority { get; set; }
    }

    internal class Factors
    {
        public double DeadlineWeight { get; set; }
        public double SkillMatchingWeight { get; set; }
        public double DependenciesWeight { get; set; }
        public double PriorityWeight { get; set; }
        public double ExperienceWeight { get; set; }
        public double AvailabilityWeight { get; set; }
    }
}