using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace IBLL
{
    public interface ISkillBLL
    {
        Task AddNewSkillAsync(SkillDTO skill);
        Task DeleteSkillAsync(int id);
        Task<List<SkillDTO>> GetAllSkillsAsync();
        Task<SkillDTO?> GetSkillByIdAsync(int id);
        Task UpdateSkillAsync(SkillDTO skill);
        Task<SkillDTO?> GetSkillByNameAsync(string name);
        Task<List<SkillDTO>> GetAllFullSkillsAsync();
    }
}
