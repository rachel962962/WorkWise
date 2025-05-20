using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface IUserBLL
    {
        Task<string?> AuthenticateAsync(string username, string password);
    }
}
