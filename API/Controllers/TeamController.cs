using BLL;
using DTO;
using IBLL;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TeamController : ControllerBase
    {
        readonly ITeamBLL teamBLL;

        public TeamController(ITeamBLL teamBLL)
        {
            this.teamBLL = teamBLL;
        }

        // GET: api/<TeamController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeamDTO>>> Get()
        {
            var teams = await teamBLL.GetAllTeamsAsync();
            return Ok(teams);
        }

        // GET api/<TeamController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TeamDTO>> Get(int id)
        {
            var team = await teamBLL.GetTeamByIdAsync(id);
            if (team == null)
            {
                return NotFound($"Team with id {id} was not found.");
            }
            return Ok(team);
        }

        // POST api/<TeamController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] TeamDTO teamDTO)
        {
            if (teamDTO == null)
            {
                return BadRequest("Team data is missing");
            }
            await teamBLL.AddNewTeamAsync(teamDTO);
            return CreatedAtAction(nameof(Get), new { id = teamDTO.TeamId }, teamDTO);
        }

        // PUT api/<TeamController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] TeamDTO teamDTO)
        {
            if (teamDTO == null)
            {
                return BadRequest("Team data is missing");
            }
            if (id != teamDTO.TeamId)
            {
                return BadRequest("Team id mismatch");
            }
            var team = await teamBLL.GetTeamByIdAsync(id);
            if (team == null)
            {
                return NotFound($"Team with id {id} was not found.");
            }
            await teamBLL.UpdateTeamAsync(teamDTO);
            return Ok(teamDTO);
        }

        // DELETE api/<TeamController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var team = await teamBLL.GetTeamByIdAsync(id);
            if (team == null)
            {
                return NotFound($"Team with id {id} was not found.");
            }
            await teamBLL.DeleteTeamAsync(id);
            return Ok($"Team with id {id} was deleted.");
        }
    }
}