using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace IBLL
{
    public interface IUserBLL
    {
        Task<AuthenticationResultDto?> AuthenticateAsync(string username, string password);
        Task<string> RegisterAsync(UserDTO user);
    }
}
