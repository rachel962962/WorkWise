using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBentities.Models;

namespace IDAL
{
    public interface IUserDAL
    {
        Task<User?> GetUserAsync(string username);
        Task<User> CreateUserAsync(User user);
        Task<bool> UsernameExistsAsync(string username);
    }
}
