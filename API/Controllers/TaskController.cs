using Microsoft.AspNetCore.Mvc;
using DTO;
using IBLL;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        readonly ITaskBLL taskBLL;
        readonly ITeamBLL teamBLL;

        public TaskController(ITaskBLL taskBLL, ITeamBLL teamBLL)
        {
            this.taskBLL = taskBLL;
            this.teamBLL = teamBLL;
        }

        // GET: api/<TaskController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDTO>>> Get()
        {
            var tasks = await taskBLL.GetAllTasksAsync();
            return Ok(tasks);
        }

        [HttpGet("Assigned")]
        public async Task<ActionResult<IEnumerable<TaskDTO>>> GetAssignedTasks()
        {
            var tasks = await taskBLL.GetAllAssignedTasksAsync();
            return Ok(tasks);
        }
        [HttpGet("Unassigned")]
        public async Task<ActionResult<IEnumerable<TaskDTO>>> GetUnassignedTasks()
        {
            var tasks = await taskBLL.GetAllUnassignedTasksAsync();
            return Ok(tasks);
        }

        [HttpGet("Completed")]
        public async Task<ActionResult<IEnumerable<TaskDTO>>> GetCompletedTasks()
        {
            var tasks = await taskBLL.GetAllCompletedTasksAsync();
            return Ok(tasks);
        }

        [HttpGet("InProgress")]
        public async Task<ActionResult<IEnumerable<TaskDTO>>> GetInProgressTasks()
        {
            var tasks = await taskBLL.GetAllInProgressTasksAsync();
            return Ok(tasks);
        }

        [HttpGet("Cancelled")]
        public async Task<ActionResult<IEnumerable<TaskDTO>>> GetCancelledTasks()
        {
            var tasks = await taskBLL.GetAllCancelledTasksAsync();
            return Ok(tasks);
        }

[HttpGet("task-required-skills/{taskId}")]
        public async Task<ActionResult<IEnumerable<SkillDTO>>> GetRequiredSkillsByTaskId(int taskId)
        {
            var task = await taskBLL.GetTaskByIdAsync(taskId);
            if (task == null)
            {
                return NotFound($"Task with id {taskId} was not found.");
            }
            var skills = await taskBLL.GetRequiredSkillsByTaskIdAsync(taskId);
            if (skills == null || skills.Count == 0)
            {
                return NotFound($"No required skills found for task with id {taskId}.");
            }
            return Ok(skills);
        }

        

        // GET api/<TaskController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDTO>> Get(int id)
        {
            var task = await taskBLL.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound($"Task with id {id} was not found.");
            }
            return Ok(task);
        }

        // POST api/<TaskController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] TaskDTO taskDTO)
        {
            if (taskDTO == null)
            {
                return BadRequest("Task data is missing");
            }
            var assignedTeam = await teamBLL.GetTeamByIdAsync(taskDTO.AssignedTeamId);
            if (assignedTeam == null)
            {
                return NotFound($"Team {taskDTO.AssignedTeamId} was not found.");
            }
            await taskBLL.AddNewTaskAsync(taskDTO);
            return CreatedAtAction(nameof(Get), new { id = taskDTO.TaskId }, taskDTO);
        }

        // PUT api/<TaskController>/5
        //[HttpPut("{id}")]
        //public async Task<ActionResult> Put(int id, [FromBody] TaskDTO taskDTO)
        //{
        //    if (taskDTO == null)
        //    {
        //        return BadRequest("Task data is missing");
        //    }
        //    if (id != taskDTO.TaskId)
        //    {
        //        return BadRequest("Task id mismatch");
        //    }
        //    var task = await taskBLL.GetTaskByIdAsync(id);
        //    if (task == null)
        //    {
        //        return NotFound($"Task with id {id} was not found.");
        //    }
        //    await taskBLL.UpdateTaskAsync(taskDTO);
        //    return Ok(taskDTO);
        //}

        // DELETE api/<TaskController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var task = await taskBLL.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound($"Task with id {id} was not found.");
            }
            await taskBLL.DeleteTaskAsync(id);
            return Ok($"Task with id {id} was deleted.");
        }
    }
}