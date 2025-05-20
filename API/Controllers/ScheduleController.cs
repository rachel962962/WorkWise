using DTO;
using IBLL;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        readonly IScheduleBLL scheduleBLL;

        public ScheduleController(IScheduleBLL scheduleBLL)
        {
            this.scheduleBLL= scheduleBLL;
        }
        // GET: api/<ScheduleController>

        [HttpPost("create-schedule")]
        public async Task<ActionResult<List<ScheduleDTO>>> CreateSchedule([FromBody] ScheduleRequestDTO scheduleRequest)
        {
            var result = await scheduleBLL.CreateNewSchedule(scheduleRequest.Workers, scheduleRequest.Tasks, scheduleRequest.End);
            return Ok(result);
        }

        // POST api/<ScheduleController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ScheduleController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ScheduleController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        [HttpGet("get-schedule-by-date/{date}")]
        public async Task<ActionResult<IEnumerable<ScheduleDTO>>> GetScheduleByDate(DateTime date)
        {
            if (date == null)
            {
                return BadRequest("Date cannot be null.");
            }
            var schedule = await scheduleBLL.GetScheduleByDateAsync(date);
            if (schedule == null)
            {
                return NotFound($"Schedule for {date} was not found.");
            }
            return Ok(schedule);
        }
    }
}
