using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBentities.Models;
using IDAL;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class TeamDAL : ITeamDAL
    {
        public async Task AddNewTeamAsync(Team team)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                await ctx.Teams.AddAsync(team);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new team", ex);
            }
        }

        public async Task DeleteTeamAsync(int id)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                var team = await ctx.Teams.FindAsync(id);
                if (team != null)
                {
                    ctx.Teams.Remove(team);
                    await ctx.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Team not found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting team", ex);
            }
        }

        public async Task<List<Team>> GetAllTeamsAsync()
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Teams.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting all teams", ex);
            }
        }

        public async Task<Team> GetTeamByIdAsync(int id)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                return await ctx.Teams.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting team by id", ex);
            }
        }

        public async Task UpdateTeamAsync(Team team)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                var existingTeam = await ctx.Teams.FindAsync(team.TeamId);
                if (existingTeam == null)
                {
                    throw new Exception("Team not found");
                }

                // Update only necessary fields
                ctx.Entry(existingTeam).CurrentValues.SetValues(team);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating team", ex);
            }
        }
        public async Task<bool> TeamExistsAsync(int teamId)
        {
            await using var ctx = new WorkWiseDbContext();
            return await ctx.Teams.AnyAsync(t => t.TeamId == teamId);
        }
    }

}
