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

        /// <summary>
        /// Calculates the cost matrix for task-worker assignments
        /// </summary>
        public static async Task<double[,]> CalculateCostMatrixAsync(List<WorkerDTO> workers, List<TaskDTO> tasks, DateTime end)
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
            double[,,] tempCostMatrix = new double[tasks.Count, workers.Count, 6];
            for (int t = 0; t < tasks.Count; t++)
            {
                for (int w = 0; w < workers.Count; w++)
                {
                    tempCostMatrix[t, w, 0] = await CalculateDeadLine(tasks[t]);
                    tempCostMatrix[t, w, 1] = await CalculateSkillMatching(tasks[t], workers[w]);
                    tempCostMatrix[t, w, 2] = await CalculateDependencies(tasks[t]);
                    tempCostMatrix[t, w, 3] = await CalculatePriority(tasks[t]);
                    tempCostMatrix[t, w, 4] = await CalculateExperience(tasks[t], workers[w]);
                    tempCostMatrix[t, w, 5] = await CalculateAvailability(tasks[t], workers[w], end);
                }
            }

            // Initialize cost matrix
            double[,] costMatrix = new double[tasks.Count, workers.Count];

            // Calculate normalization factors
            var normalizationFactors = await CalculateNormalizationFactorsAsync(tempCostMatrix);
            tempCostMatrix = Normalize(tempCostMatrix, normalizationFactors);
            Factors factors = CalculateFactors(tempCostMatrix);
            // Calculate cost for each task-worker pair
            for (int t = 0; t < tasks.Count; t++)
            {
                for (int w = 0; w < workers.Count; w++)
                {
                    costMatrix[t, w] = await CalculateCostForTaskWorkerPairAsync(ExtractSlice(tempCostMatrix, t, w), factors);
                }
            }

            return costMatrix;
        }
        //חישוב דד-ליין לפני נרמול
        private static async Task<double> CalculateDeadLine(TaskDTO task)
        {
            return await Task.FromResult((task.Deadline - DateTime.Now).TotalDays);
        }
        //חישוב התאמה בין משימה לעובד לפני נרמול
        private static async Task<double> CalculateSkillMatching(TaskDTO task, WorkerDTO worker)
        {
            // Get required skills for the task
            var requiredSkillIds = await ITaskBLL.GetRequiredSkillsByTaskIdAsync(task.TaskId);
            // Get worker's skills
            var workerSkills = await IWorkerBLL.GetSkillsByWorkerIdAsync(worker.WorkerId);
            // Calculate matching skills
            int matchingSkills = requiredSkillIds.Count(skillId =>
                workerSkills.Any(ws => ws.SkillId == skillId.SkillId));
            return requiredSkillIds.Any()
                ? (double)matchingSkills / requiredSkillIds.Count()
                : 0;
        }

        //חישוב תלויות לפני נרמול
        private static async Task<double> CalculateDependencies(TaskDTO task)
        {
            // Get dependencies for the task
            var dependencies = await ITaskBLL.GetDependenciesByTaskIdAsync(task.TaskId);
            return (double)dependencies.Count();
        }
        //חישוב עדיפות לפני נרמול
        private static async Task<double> CalculatePriority(TaskDTO task)
        {
            return await Task.FromResult((double)task.PriorityLevel);
        }
        //חישוב התאמה בין רמת הקושי של המשימה לרמת המומחיות של העובד
        private static async Task<double> CalculateExperience(TaskDTO task, WorkerDTO worker)
        {
            // Get required skills for the task
            var requiredSkillIds = await ITaskBLL.GetRequiredSkillsByTaskIdAsync(task.TaskId);
            // Get worker's skills with proficiency
            var workerSkills = await IWorkerBLL.GetSkillsByWorkerIdAsync(worker.WorkerId);
            double sum = 0;
            int complexity = (int)task.ComplexityLevel;
            foreach (var skill in requiredSkillIds)
            {
                if (workerSkills.Contains(skill))
                {
                    var workerSkill = workerSkills.Find(ws => ws.SkillId == skill.SkillId);
                    var proficiencyTask = IWorkerSkillBLL.GetProficiencyLevelBySkillAndWorkerId(workerSkill.SkillId, worker.WorkerId);
                    ProficiencyLevel proficiency = await proficiencyTask;
                    int proficiencyLevel;
                    switch (proficiency)
                    {
                        case ProficiencyLevel.מתחיל:
                            proficiencyLevel = 1;
                            break;
                        case ProficiencyLevel.בינוני:
                            proficiencyLevel = 2;
                            break;
                        case ProficiencyLevel.מומחה:
                            proficiencyLevel = 3;
                            break;
                        default:
                            proficiencyLevel = 0;
                            break;
                    }
                    if (proficiencyLevel == complexity - 1)
                    {
                        sum += 0.5;
                    }
                    else if (!(proficiencyLevel >= complexity))
                    {
                        sum += 1;
                    }
                }
            }
            // Experience calculation based on proficiency
            return sum / requiredSkillIds.Count();
        }
        //חישוב זמינות
        private static async Task<double> CalculateAvailability(TaskDTO task, WorkerDTO worker, DateTime end)
        {
            DateTime start = DateTime.Now;
            decimal hours = 0;
            var workerAvailability = await IWorkerBLL.GetWokerAvailabilityByIdAsync(worker.WorkerId);
            List<WorkDay> workDays = new List<WorkDay>();
            foreach (var day in workerAvailability)
            {
                workDays.Add(day.WorkDay);
            }
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
            if ((task.Duration / hours) >= 1)
            {
                return 1;
            }
            return (double)(task.Duration / hours);
        }
        private static double[,,] Normalize(double[,,] mat, NormalizationFactors factors)
        {
            int d = factors.MaxDeadline - factors.MinDeadline;
            for (int t = 0; t < mat.GetLength(0); t++)
            {
                for (int w = 0; w < mat.GetLength(1); w++)
                {
                    mat[t, w, 0] = (mat[t, w, 0] - factors.MinDeadline) / d;
                    mat[t, w, 2] = mat[t, w, 2] / factors.MaxDependencies;
                    mat[t, w, 3] = mat[t, w, 3] / factors.MaxPriority;
                }
            }
            return mat;
        }
        public static double CalculateDeadlineWeight(double[,,] mat)
        {
            double sum = 0;
            int dim1 = mat.GetLength(0);
            int dim2 = mat.GetLength(1);
            for (int i = 0; i < dim1; i++)
            {
                for (int j = 0; j < dim2; j++)
                {
                    sum+= 1- mat[i, j, 0];
                }
            }
            return sum/dim1;
        }
        public static double CalculateSkillMatchingWeight(double[,,] mat)
        {
            double sum = 0;
            int dim1 = mat.GetLength(0);
            int dim2 = mat.GetLength(1);
            for (int i = 0; i < dim1; i++)
            {
                for (int j = 0; j < dim2; j++)
                {
                    sum += mat[i, j, 1];
                }
            }
            return 1-(sum/dim1*dim2);
        }
        public static double CalculateDependenciesWeight(double[,,] mat)
        {
            double sum = 0;
            int dim1 = mat.GetLength(0);
            int dim2 = mat.GetLength(1);
            for (int i = 0; i < dim1; i++)
            {
                for (int j = 0; j < dim2; j++)
                {
                    sum += mat[i, j, 2];
                }
            }
            return sum / dim1;
        }
        public static double CalculatePriorityWeight(double[,,] mat)
        {
            double sum = 0;
            int dim1 = mat.GetLength(0);
            int dim2 = mat.GetLength(1);
            for (int i = 0; i < dim1; i++)
            {
                for (int j = 0; j < dim2; j++)
                {
                    sum += mat[i, j, 3];
                }
            }
            return sum / dim1;
        }
        public static double CalculateExperienceWeight(double[,,] mat)
        {
            double sum = 0;
            int dim1 = mat.GetLength(0);
            int dim2 = mat.GetLength(1);
            for (int i = 0; i < dim1; i++)
            {
                for (int j = 0; j < dim2; j++)
                {
                    sum += mat[i, j, 4];
                }
            }
            return sum /( dim1*dim2);
        }
        public static double CalculateAvailabilityWeight(double[,,] mat)
        {
            double sum = 0;
            int dim1 = mat.GetLength(0);
            int dim2 = mat.GetLength(1);
            for (int i = 0; i < dim1; i++)
            {
                for (int j = 0; j < dim2; j++)
                {
                    sum += mat[i, j, 5];
                }
            }
            return sum / (dim1 * dim2);
        }
        private static Factors CalculateFactors(double[,,]mat)
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
            double cost = 0;
            cost += mat[0] * factors.DeadlineWeight;
            cost += mat[1] * factors.SkillMatchingWeight;
            cost += mat[2] * factors.DependenciesWeight;
            cost += mat[3] * factors.PriorityWeight;
            cost += mat[4] * factors.ExperienceWeight;
            cost += mat[5] * factors.AvailabilityWeight;
            return await Task.FromResult(cost);
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
        //בדיקה האם העובד מתוכנן ביום נתון
        public static async Task<bool> CheckIfWorkerIsSchedule(WorkerDTO worker, DateTime day)
        {
            var workerSchedule = await IWorkerBLL.GetWokerScheduleByIdAsync(worker.WorkerId);
            foreach (var item in workerSchedule)
            {
                if (day >= item.StartTime && day <= item.FinishTime)
                    return true;
            }
            return false;
        }
        public static async Task<bool> CheckIfWorkerIsAbsence(WorkerDTO worker, DateTime day)
        {
            var workerAbsence = await IWorkerBLL.GetWokerAbsenceByIdAsync(worker.WorkerId);
            foreach (var item in workerAbsence)
            {
                if (day >= item.StartDate && day <= item.EndDate)
                    return true;
            }
            return false;
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
                    max = Math.Max(max, mat[i, j, z]);
                }
            }
            return max;
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
                    min = Math.Min(min, mat[i, j, z]);
                }
            }
            return min;
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
        private static async Task<NormalizationFactors> CalculateNormalizationFactorsAsync(double[,,] mat)
        {
            // Get max dependencies across all tasks
            double maxDeadline = GetMax(mat, 0).Result;
            double maxDependencies = GetMax(mat, 2).Result;
            double maxPriority = GetMax(mat, 3).Result;
            double minDeadline = GetMin(mat, 0).Result;
            return new NormalizationFactors
            {
                MinDeadline = (int)minDeadline,
                MaxDeadline = (int)maxDeadline,
                MaxDependencies = (int)maxDependencies,
                MaxPriority = (int)maxPriority
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

