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
    public class SkillController : ControllerBase
    {
        readonly ISkillBLL skillBLL;

        public SkillController(ISkillBLL skillBLL)
        {
            this.skillBLL = skillBLL;
        }

        // GET: api/<SkillController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            var skills = await skillBLL.GetAllSkillsAsync();
            return Ok(skills);
        }

        [HttpGet("fullSkills")]
        public async Task<ActionResult<IEnumerable<SkillDTO>>> GetFullSkills()
        {
            var skills = await skillBLL.GetAllFullSkillsAsync();
            return Ok(skills);
        }


        // GET api/<SkillController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SkillDTO>> Get(int id)
        {
            var skill = await skillBLL.GetSkillByIdAsync(id);
            if (skill == null)
            {
                return NotFound($"Skill with id {id} was not found.");
            }
            return Ok(skill);
        }

        // POST api/<SkillController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] SkillDTO skill)
        {
            if (skill == null)
            {
                return BadRequest("Skill data is missing");
            }
            var skill1 = await skillBLL.GetSkillByNameAsync(skill.Name);
            if (skill1 != null)
            {
                return BadRequest($"Skill with name {skill.Name} already exists.");
            }
            await skillBLL.AddNewSkillAsync(skill);
            return CreatedAtAction(nameof(Get), new { id = skill.SkillId }, skill);
        }

        // PUT api/<SkillController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] SkillDTO skill)
        {
            if (skill == null)
            {
                return BadRequest("Skill data is missing");
            }
            if (id != skill.SkillId)
            {
                return BadRequest("Skill id mismatch");
            }
            var skilll = await skillBLL.GetSkillByIdAsync(id);
            if (skilll == null)
            {
                return NotFound($"Skill with id {id} was not found.");
            }
            await skillBLL.UpdateSkillAsync(skill);
            return Ok(skill);
        }

        // DELETE api/<SkillController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var skill = await skillBLL.GetSkillByIdAsync(id);
            if (skill == null)
            {
                return NotFound($"Skill with id {id} was not found.");
            }
            await skillBLL.DeleteSkillAsync(id);
            return Ok($"Skill with id {id} was deleted.");
        }
    }
}