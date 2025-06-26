using DTO;
using IBLL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        [HttpPost("manual-assignments")]
        public async Task<ActionResult> Post([FromBody] List<TaskAssignmentDto> assignments)
        {
            if (assignments == null || !assignments.Any())
            {
                return BadRequest("Assignments cannot be null or empty.");
            }
            try
            {
                var schedules = await scheduleBLL.ManualAssignments(assignments);
                return Ok(new
                {
                    message = "Assignments successfully processed.",
                    count = schedules.Count,
                    schedules
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public class StatusUpdateModel
        {
            public string Status { get; set; }
        }

        [HttpPut("{scheduleId}/status")]
        public async Task<ActionResult> UpdateScheduleStatus(int scheduleId, [FromBody] StatusUpdateModel model)
        {
            try
            {
                await scheduleBLL.UpdateScheduleStatusAsync(scheduleId, model.Status );
                return Ok("Schedule status updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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

        [HttpGet("get-schedule-by-date-and-team/{date}/{teamId}")]
        public async Task<ActionResult<IEnumerable<ScheduleDTO>>> GetScheduleByDateAndTeam(DateTime date, int teamId)
        {
            if (date == null)
            {
                return BadRequest("Date cannot be null.");
            }
            if (teamId == 0)
            {
                return BadRequest("Team ID cannot be zero.");
            }
            var schedule = await scheduleBLL.GetScheduleByDateAndTeamAsync(date, teamId);
            if (schedule == null)
            {
                return NotFound($"Schedule for {date} and team {teamId} was not found.");
            }
            return Ok(schedule);
        }

        [HttpGet("get-assignments-count-for-today-by-team/{teamId}")]
        public async Task<ActionResult<int>> GetAssignmentsCountByDateAndTeam( int teamId)
        {
            if (teamId == 0)
            {
                return BadRequest("Team ID cannot be zero.");
            }
            var count = await scheduleBLL.GetAssignScheduleForTodayByTeamAsync(teamId);
            return Ok(count);
        }

        [HttpGet("get-all-ongoing-schedules-by-team{teamId}")]
        public async Task<ActionResult<IEnumerable<ScheduleDTO>>> GetAllOngoingSchedulesByTeamAndDate(int teamId)
        {
            if (teamId == 0)
            {
                return BadRequest("Team ID cannot be zero.");
            }
            var count = await scheduleBLL.GetAllOngoingSchedulesCountByTeamAndDateAsync(teamId);
            return Ok(count);
        }

        //מספר משימות שבוטלו היום לפי צוות

        [HttpGet("get-cancelled-tasks-count-for-today-by-team/{teamId}")]
        public async Task<ActionResult<int>> GetCancelledTasksCountForTodayByTeam(int teamId)
        {
            if (teamId == 0)
            {
                return BadRequest("Team ID cannot be zero.");
            }
            var count = await scheduleBLL.GetCancelledTasksCountForTodayByTeamAsync(teamId);
            return Ok(count);
        }
    }
}
