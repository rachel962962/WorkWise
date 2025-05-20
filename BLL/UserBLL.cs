using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Auth;
using IBLL;
using IDAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;

namespace BLL
{
    public class UserBLL : IUserBLL
    {
        private readonly IUserDAL userDAL;
        private readonly IJwtTokenService jwtTokenService;
        private readonly PasswordHasher<string> passwordHasher = new PasswordHasher<string>();

        public UserBLL(IUserDAL userDAL, IJwtTokenService jwtTokenService)
        {
            this.userDAL = userDAL;
            this.jwtTokenService = jwtTokenService;
        }

        public async Task<string?> AuthenticateAsync(string username, string password)
        {
            var user = await userDAL.GetUserAsync(username);
            if (user == null)
            {
                return null;
            }
            var verificationResult = passwordHasher.VerifyHashedPassword(user.Username, user.PasswordHash, password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return null;
            }
            var token = jwtTokenService.GenerateToken(user.Username);
            return token;

        }
    }
}
