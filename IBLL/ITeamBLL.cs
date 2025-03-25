using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBentities.Models;
using DTO;

namespace IBLL
{
    public interface ITeamBLL
    {
        Task AddNewTeamAsync(TeamDTO team);
        Task DeleteTeamAsync(int id);
        Task<List<TeamDTO>> GetAllTeamsAsync();
        Task<TeamDTO?> GetTeamByIdAsync(int id);
        Task UpdateTeamAsync(TeamDTO team);
    }
}
