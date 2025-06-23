using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBentities.Models;

namespace IDAL
{
    public interface ITeamDAL
    {
        Task AddNewTeamAsync(Team team);
        Task<List<Team>> GetAllTeamsAsync();
        Task UpdateTeamAsync(Team team);
        Task<Team> GetTeamByIdAsync(int id);
        Task DeleteTeamAsync(int id);
        Task<bool> TeamExistsAsync(int teamId);

    }
}
