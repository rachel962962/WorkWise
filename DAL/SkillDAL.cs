using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DBentities.Models;
using IDAL;
using DTO;

namespace DAL
{
    public class SkillDAL : ISkillDAL
    {
        public async Task AddNewSkillAsync(Skill skill)
        {
            using WorkWiseDbContext ctx = new WorkWiseDbContext();
            try
            {
                await ctx.Skills.AddAsync(skill);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new skill", ex);
            }
        }

        public async Task DeleteSkillAsync(int id)
        {
            using WorkWiseDbContext ctx = new WorkWiseDbContext();
            try
            {
                var skill = await ctx.Skills.FindAsync(id);
                if (skill != null)
                {
                    ctx.Skills.Remove(skill);
                    await ctx.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Skill not found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting skill", ex);
            }
        }

        public async Task<List<Skill>> GetAllSkillsAsync()
        {
            using WorkWiseDbContext ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Skills.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting all skills", ex);
            }
        }

        public async Task<Skill> GetSkillByIdAsync(int id)
        {
            using WorkWiseDbContext ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Skills.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting skill by id", ex);
            }
        }

        public async Task UpdateSkillAsync(Skill skill)
        {
            using WorkWiseDbContext ctx = new WorkWiseDbContext();
            try
            {
                var existingSkill = await ctx.Skills.FindAsync(skill.SkillId);
                if (existingSkill == null)
                {
                    throw new Exception("Skill not found");
                }
                ctx.Entry(existingSkill).CurrentValues.SetValues(skill);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating skill", ex);
            }
        }
        public async Task<Skill?> GetSkillByNameAsync(string name)
        {
            using WorkWiseDbContext ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Skills.FirstOrDefaultAsync(s => s.Name == name);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting skill by name", ex);

            }
        }
    }
}