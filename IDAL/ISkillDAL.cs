using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBentities.Models;

namespace IDAL
{
    public interface ISkillDAL
    {
        Task AddNewSkillAsync(Skill skill);
        Task DeleteSkillAsync(int id);
        Task<List<Skill>> GetAllSkillsAsync();
        Task<Skill> GetSkillByIdAsync(int id);
        Task UpdateSkillAsync(Skill skill);
        Task<Skill?> GetSkillByNameAsync(string name);
    }
}
