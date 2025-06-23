using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BLL.Auth;
using DBentities.Models;
using DTO;
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
        private readonly IMapper mapper;

        public UserBLL(IUserDAL userDAL, IJwtTokenService jwtTokenService)
        {
            this.userDAL = userDAL;
            this.jwtTokenService = jwtTokenService;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserDTO>().ReverseMap();
            });
            this.mapper = config.CreateMapper();
        }

        public async Task<AuthenticationResultDto?> AuthenticateAsync(string username, string password)
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

            return new AuthenticationResultDto
            {
                Token = token,
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role
            };

        }

        public async Task<string> RegisterAsync(UserDTO user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var hashedPassword = passwordHasher.HashPassword(user.Username, user.Password);
            user.Password = hashedPassword;
            User createdUser = await userDAL.CreateUserAsync(mapper.Map<User>(user));
            var token = jwtTokenService.GenerateToken(createdUser.Username);
            return token;
        }
    }
}
