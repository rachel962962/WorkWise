using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DBentities.Models;
using DTO;
using IBLL;
using IDAL;
using Microsoft.AspNetCore.Identity;

namespace BLL
{
    public class WorkerBLL : IWorkerBLL
    {
        private readonly IWorkerDAL workerDAL;
        private readonly ITeamDAL teamDAL;
        private readonly IUserDAL userDAL;
        private readonly ISkillDAL skillDAL;
        private readonly IMapper mapper;
        private readonly PasswordHasher<string> passwordHasher = new PasswordHasher<string>();

        public WorkerBLL(IWorkerDAL workerDAL, ITeamDAL teamDAL, IUserDAL userDAL, ISkillDAL skillDAL)
        {
            this.workerDAL = workerDAL;
            this.teamDAL = teamDAL;
            this.userDAL = userDAL;
            this.skillDAL = skillDAL;

            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Worker, WorkerDTO>()
                    .ForMember(dest => dest.Skills, opt => opt.MapFrom(src =>
                        src.WorkerSkills.Select(ws => ws.Skill)))
                    .ReverseMap()
                    .ForMember(dest => dest.WorkerSkills, opt => opt.MapFrom(src =>
                        src.Skills.Select(skillDto => new WorkerSkill
                        {
                            SkillId = skillDto.SkillId
                        })));
                cfg.CreateMap<Skill, SkillDTO>().ReverseMap();
                cfg.CreateMap<WorkerAvailability, WorkerAvailabilityDTO>().ReverseMap();
                cfg.CreateMap<WorkerAbsence, WorkerAbsenceDTO>().ReverseMap();
                cfg.CreateMap<Schedule, ScheduleDTO>().ReverseMap();

            });

            mapper = new Mapper(configTaskConverter);
        }

        public async Task AddNewWorkerAsync(WorkerDTO worker)
        {
            await workerDAL.AddNewWorkerAsync(mapper.Map<Worker>(worker));
        }
        public async Task<WorkerResponseDto> CreateWorkerAsync(WorkerCreationDto workerDto)
        {
            // Validate team exists if specified
            if (workerDto.TeamId.HasValue)
            {
                var teamExists = await teamDAL.TeamExistsAsync(workerDto.TeamId.Value);
                if (!teamExists)
                {
                    throw new ArgumentException($"Team with ID {workerDto.TeamId.Value} does not exist");
                }
            }

            // Validate username is unique
            var usernameExists = await userDAL.UsernameExistsAsync(workerDto.User.Username);
            if (usernameExists)
            {
                throw new ArgumentException($"Username '{workerDto.User.Username}' is already taken");
            }

            // Validate skills exist
            if (workerDto.Skills != null)
            {
                foreach (var skill in workerDto.Skills)
                {
                    var skillExists = await skillDAL.SkillExistsAsync(skill.SkillId);
                    if (!skillExists)
                    {
                        throw new ArgumentException($"Skill with ID {skill.SkillId} does not exist");
                    }
                }
            }

            // Create user with hashed password
            var user = new User
            {
                Username = workerDto.User.Username,
                PasswordHash = passwordHasher.HashPassword(null, workerDto.User.Password),
                Role = workerDto.User.Role
            };

            // Create worker
            var worker = new Worker
            {
                FirstName = workerDto.FirstName,
                LastName = workerDto.LastName,
                TeamId = workerDto.TeamId,
                DailyHours = workerDto.DailyHours,
                MaxWeeklyHours = workerDto.MaxWeeklyHours
            };

            // Create availabilities
            var availabilities = new List<WorkerAvailability>();
            if (workerDto.AvailableDays != null)
            {
                foreach (var day in workerDto.AvailableDays)
                {
                    availabilities.Add(new WorkerAvailability
                    {
                        WorkDay = day
                    });
                }
            }

            // Create skills
            var skills = new List<WorkerSkill>();
            if (workerDto.Skills != null)
            {
                foreach (var skill in workerDto.Skills)
                {
                    skills.Add(new WorkerSkill
                    {
                        SkillId = skill.SkillId,
                        ProficiencyLevel = skill.ProficiencyLevel.ToString(),
                    });
                }
            }

            // Save worker with all related entities
            var createdWorker = await workerDAL.CreateWorkerAsync(worker, availabilities, skills, user);

            // Map to response DTO
            return await GetWorkerByIdAsync(createdWorker.WorkerId);
        }

        public async Task<WorkerResponseDto> GetWorkerByIdAsync(int workerId)
        {
            var worker = await workerDAL.GetWorkerByIdAsync(workerId);
            if (worker == null)
            {
                throw new ArgumentException($"Worker with ID {workerId} does not exist");
            }

            return new WorkerResponseDto
            {
                WorkerId = worker.WorkerId,
                FirstName = worker.FirstName,
                LastName = worker.LastName,
                TeamId = worker.TeamId,
                TeamName = worker.Team?.Name,
                DailyHours = worker.DailyHours,
                MaxWeeklyHours = worker.MaxWeeklyHours,
                AvailableDays = worker.WorkerAvailabilities?.Select(a => a.WorkDay).ToList() ?? new List<string>(),
                Skills = worker.WorkerSkills?.Select(s => new WorkerSkillDetailDto
                {
                    SkillId = s.SkillId,
                    SkillName = s.Skill.Name,
                    ProficiencyLevel = s.ProficiencyLevel
                }).ToList() ?? new List<WorkerSkillDetailDto>()
            };
        }

        public async Task<WorkerResponseDto?> GetWorkerByUserIdAsync(int userId)
        {
            var worker = await workerDAL.GetWorkerByUserIdAsync(userId);
            if (worker == null)
            {
                return null;
            }

            return new WorkerResponseDto
            {
                WorkerId = worker.WorkerId,
                FirstName = worker.FirstName,
                LastName = worker.LastName,
                TeamId = worker.TeamId,
                TeamName = worker.Team?.Name,
                DailyHours = worker.DailyHours,
                MaxWeeklyHours = worker.MaxWeeklyHours,
                AvailableDays = worker.WorkerAvailabilities?.Select(a => a.WorkDay).ToList() ?? new List<string>(),
                Skills = worker.WorkerSkills?.Select(s => new WorkerSkillDetailDto
                {
                    SkillId = s.SkillId,
                    SkillName = s.Skill.Name,
                    ProficiencyLevel = s.ProficiencyLevel
                }).ToList() ?? new List<WorkerSkillDetailDto>()
            };
        }

        public async Task DeleteWorkerAsync(int id)
        {
            await workerDAL.DeleteWorkerAsync(id);
        }

        public async Task<List<SkillDTO>> GetSkillsByWorkerIdAsync(int workerId)
        {
            var skills =  await workerDAL.GetSkillsByWorkerIdAsync(workerId);
            return mapper.Map<List<SkillDTO>>(skills);
        }

        public async Task<List<WorkerAbsenceDTO>> GetWokerAbsenceByIdAsync(int workerId)
        {
            var list =await workerDAL.GetWokerAbsenceByIdAsync(workerId);
            return mapper.Map<List<WorkerAbsenceDTO>>(list);
        }

        public async Task<List<WorkerAvailabilityDTO>> GetWokerAvailabilityByIdAsync(int workerId)
        {
            var list =await workerDAL.GetWokerAvailabilityByIdAsync(workerId);
            return mapper.Map<List<WorkerAvailabilityDTO>>(list);
        }

        public async Task<List<ScheduleDTO>> GetWokerScheduleByIdAsync(int workerId)
        {
            var list =await workerDAL.GetWokerScheduleByIdAsync(workerId);
            return mapper.Map<List<ScheduleDTO>>(list);
        }

        //public async Task<WorkerDTO> GetWorkerByIdAsync(int id)
        //{
        //    var worker = await workerDAL.GetWorkerByIdAsync(id);
        //    return worker != null ? mapper.Map<WorkerDTO>(worker) : null;
        //}

        public async Task<List<WorkerDTO>> GetWorkersAsync()
        {
            var list = await workerDAL.GetWorkersAsync();
            return mapper.Map<List<WorkerDTO>>(list);
        }

        public async Task UpdateWorkerAsync(WorkerDTO worker)
        {
            Worker worker1 = mapper.Map<Worker>(worker);
            await workerDAL.UpdateWorkerAsync(worker1);
        }

        public async Task<List<WorkerDTO>> GetWorkersByTeamIdAsync(int teamId)
        {
            var workers =await workerDAL.GetWorkersByTeamIdAsync(teamId);
            return mapper.Map<List<WorkerDTO>>(workers);

        }

        public async Task<List<WorkerAvailabilityDTO>> GetWokerAvailabilityAsync()
        {
            var list = await workerDAL.GetWokerAvailabilityAsync();
            return mapper.Map<List<WorkerAvailabilityDTO>>(list);
        }

        public async Task<List<WorkerAbsenceDTO>> GetAllWokerAbsenceAsync()
        {
            var list = await workerDAL.GetAllWokerAbsenceAsync();
            return mapper.Map<List<WorkerAbsenceDTO>>(list);
        }
    }
}
