using DTO;
using IBLL;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserBLL userBLL;
        public UserController(IUserBLL userBLL) {
            this.userBLL = userBLL;
        }
        // GET: api/<UserController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<UserController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<UserController>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserDTO user)
        {
            if (user == null)
            {
                return BadRequest("User cannot be null.");
            }
            var token = await userBLL.AuthenticateAsync(user.Username, user.PasswordHash);
            if (token == null)
            {
                return Unauthorized("Invalid username or password.");
            }
            return Ok(token);
        }

        // PUT api/<UserController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
