using DBentities.Models;
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
    public class UserController : ControllerBase
    {
        private readonly IUserBLL userBLL;
        private readonly IWorkerBLL workerBLL;
        public UserController(IUserBLL userBLL, IWorkerBLL workerBLL) {
            this.userBLL = userBLL;
            this.workerBLL = workerBLL;
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
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserDTO user)
        {
            if (user == null)
            {
                return BadRequest("User cannot be null.");
            }

            var loginResult = await userBLL.AuthenticateAsync(user.Username, user.Password);

            if (loginResult == null || string.IsNullOrEmpty(loginResult.Token))
            {
                return Unauthorized("Invalid username or password.");
            }

            WorkerResponseDto? worker = null;
            if (loginResult.UserId.HasValue)
            {
                try
                {
                    worker = await workerBLL.GetWorkerByUserIdAsync(loginResult.UserId.Value);
                }
                catch (Exception)
                {
                    worker = null;
                }
            }

            return Ok(new
            {
                Success = true,
                Message = "Login successful.",
                Token = loginResult.Token,
                User = new
                {
                    UserId = loginResult.UserId,
                    Username = loginResult.Username,
                    Role = loginResult.Role
                },
                Worker = worker 
            });
        }
      

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDTO user)
        {
            if (user == null)
            {
                return BadRequest("User cannot be null.");
            }
            var token = await userBLL.RegisterAsync(user);
            if (token == null)
            {
                return BadRequest("Registration failed.");
            }
            return Ok(new
            {
                message = "Registration successful.",
                token = token
            });
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
