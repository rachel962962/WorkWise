using BLL;
using DAL;
using DTO;
using IBLL;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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

        // GET api/<WorkerController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            var worker = await workerBLL.GetWorkerByIdAsync(id);
            if (worker == null)
            {
                return NotFound($"Worker with id {id} was not found.");
            }
            return Ok(worker);
        }

        // POST api/<WorkerController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] WorkerDTO worker)
        {
            if (worker == null)
            {
                return BadRequest("Worker data is missing");
            }
            var team = await teamBLL.GetTeamByIdAsync(worker.TeamId); // Use the injected ITeamBLL
            if (team == null)
            {
                return NotFound($"Team {worker.TeamId} was not found.");
            }
            await workerBLL.AddNewWorkerAsync(worker);
            return CreatedAtAction(nameof(Get), new { id = worker.WorkerId }, worker);
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
    }
}