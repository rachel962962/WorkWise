using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DAL;
using DBentities.Models;
using DTO;
using IBLL;
using IDAL;

namespace BLL
{
    public class TeamBLL : ITeamBLL
    {
        private readonly ITeamDAL teamDAL;
        private readonly IMapper mapper;

        public TeamBLL(ITeamDAL team)
        {
            this.teamDAL = team;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Team, TeamDTO>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
        }

        public async Task AddNewTeamAsync(TeamDTO team)
        {
            await teamDAL.AddNewTeamAsync(mapper.Map<Team>(team));
        }

        public async Task DeleteTeamAsync(int id)
        {
            await teamDAL.DeleteTeamAsync(id);
        }

        public async Task<List<TeamDTO>> GetAllTeamsAsync()
        {
            var list = await teamDAL.GetAllTeamsAsync();
            return mapper.Map<List<TeamDTO>>(list);
        }

        public async Task<TeamDTO?> GetTeamByIdAsync(int id)
        {
            var team = await teamDAL.GetTeamByIdAsync(id);
            return team != null ? mapper.Map<TeamDTO>(team) : null;
        }

        public async Task UpdateTeamAsync(TeamDTO team)
        {
            Team team1 = mapper.Map<Team>(team);
            await teamDAL.UpdateTeamAsync(team1);
        }
    }
}
