using BLL;
using DAL;
using DTO;
using IBLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkerController : ControllerBase
    {
        readonly IWorkerBLL workerBLL;
        readonly ITeamBLL teamBLL;

        public WorkerController(IWorkerBLL workerBLL, ITeamBLL teamBLL)
        {
            this.workerBLL = workerBLL;
            this.teamBLL = teamBLL;
        }

        // GET: api/<WorkerController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkerDTO>>> Get()
        {
            var workers = await workerBLL.GetWorkersAsync();
            return Ok(workers);
        }

        [HttpGet("workers-by-team/{teamId}")]
        public async Task<ActionResult<IEnumerable<WorkerDTO>>> GetWorkersByTeam(int teamId)
        {
            if (teamId == 0)
            {
                return BadRequest("Team ID cannot be zero.");
            }
            var team = await teamBLL.GetTeamByIdAsync(teamId);
            if (team == null)
            {
                return NotFound($"Team with id {teamId} was not found.");
            }
            var workers = await workerBLL.GetWorkersByTeamIdAsync(teamId);
            return Ok(workers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WorkerResponseDto>> GetWorker(int id)
        {
            try
            {
                var worker = await workerBLL.GetWorkerByIdAsync(id);
                return Ok(worker);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<WorkerResponseDto>> CreateWorker([FromBody] WorkerCreationDto workerDto)
        {
            try
            {
                var result = await workerBLL.CreateWorkerAsync(workerDto);
                return CreatedAtAction(nameof(GetWorker), new { id = result.WorkerId }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // PUT api/<WorkerController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] WorkerDTO workerDTO)
        {
            if (workerDTO == null)
            {
                return BadRequest("Worker data is missing");
            }
            if (id != workerDTO.WorkerId)
            {
                return BadRequest("Worker id mismatch");
            }
            var team = await teamBLL.GetTeamByIdAsync(workerDTO.TeamId); // Use the injected ITeamBLL
            if (team == null)
            {
                return NotFound($"Team {workerDTO.TeamId} was not found.");
            }
            var worker = await workerBLL.GetWorkerByIdAsync(id);
            if (worker == null)
            {
                return NotFound($"Worker with id {id} was not found.");
            }
            await workerBLL.UpdateWorkerAsync(workerDTO);
            return Ok(workerDTO);
        }

        // DELETE api/<WorkerController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var worker = await workerBLL.GetWorkerByIdAsync(id);
            if (worker == null)
            {
                return NotFound($"Worker with id {id} was not found.");
            }
            await workerBLL.DeleteWorkerAsync(id);
            return Ok($"Worker with id {id} was deleted.");
        }

        [HttpGet("today-worker-absence-count-by-team/{teamId}")]
        public async Task<ActionResult<int>> GetTodayWorkerAbsenceCountByTeam(int teamId)
        {
            if (teamId == 0)
            {
                return BadRequest("Team ID cannot be zero.");
            }
            var team = await teamBLL.GetTeamByIdAsync(teamId);
            if (team == null)
            {
                return NotFound($"Team with id {teamId} was not found.");
            }
            var count = await workerBLL.GetWokerAbsenceCountByTeamAsync(teamId);
            return Ok(count);
        }
    }
}