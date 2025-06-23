using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class AuthenticationResultDto
    {
        public string Token { get; set; } = null!;
        public int? UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
