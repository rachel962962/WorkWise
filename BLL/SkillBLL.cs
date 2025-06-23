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

namespace BLL
{
    public class SkillBLL : ISkillBLL
    {
        private readonly ISkillDAL skillDAL;
        private readonly IMapper mapper;

        public SkillBLL(ISkillDAL skillDAL)
        {
            this.skillDAL = skillDAL;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Skill, SkillDTO>().ReverseMap();

            });
            mapper = new Mapper(configTaskConverter);
        }

        public async Task AddNewSkillAsync(SkillDTO skill)
        {
            Skill skill1 = mapper.Map<Skill>(skill);
            await skillDAL.AddNewSkillAsync(skill1);
        }

        public async Task DeleteSkillAsync(int id)
        {
            await skillDAL.DeleteSkillAsync(id);
        }

        public async Task<List<SkillDTO>> GetAllSkillsAsync()
        {
            var list = await skillDAL.GetAllSkillsAsync();
            return mapper.Map<List<SkillDTO>>(list);
        }

        public async Task<SkillDTO?> GetSkillByIdAsync(int id)
        {
            var skill = await skillDAL.GetSkillByIdAsync(id);
            return skill != null ? mapper.Map<SkillDTO>(skill) : null;
        }

        public async Task UpdateSkillAsync(SkillDTO skill)
        {
            Skill skill1 = mapper.Map<Skill>(skill);
            await skillDAL.UpdateSkillAsync(skill1);
        }
        public async Task<SkillDTO?> GetSkillByNameAsync(string name)
        {
            var skill = await skillDAL.GetSkillByNameAsync(name);
            return skill != null ? mapper.Map<SkillDTO>(skill) : null;
        }

        public async Task<List<SkillDTO>?> GetAllFullSkillsAsync()
        {
            var skills = await skillDAL.GetAllFullSkillsAsync();
            return skills != null ? mapper.Map<List<SkillDTO>>(skills) : null;
        }
    }
}
