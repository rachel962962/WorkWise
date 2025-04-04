﻿using Microsoft.AspNetCore.Mvc;
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
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] TaskDTO taskDTO)
        {
            if (taskDTO == null)
            {
                return BadRequest("Task data is missing");
            }
            if (id != taskDTO.TaskId)
            {
                return BadRequest("Task id mismatch");
            }
            var task = await taskBLL.GetTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound($"Task with id {id} was not found.");
            }
            await taskBLL.UpdateTaskAsync(taskDTO);
            return Ok(taskDTO);
        }

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