﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBentities.Models;
using IDAL;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class UserDAL : IUserDAL
    {
        public async Task<User?> GetUserAsync(string username)
        {
            await using var ctx = new WorkWiseDbContext();
            try
            {
                var user = await ctx.Users
                    .Where(u => u.Username == username)
                    .FirstOrDefaultAsync();
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting user", ex);
            }

        }

        public async Task<User> CreateUserAsync(User user)
        {
            await using var ctx = new WorkWiseDbContext();
            await ctx.Users.AddAsync(user);
            await ctx.SaveChangesAsync();
            return user;
        }
        public async Task<bool> UsernameExistsAsync(string username)
        {
            await using var ctx = new WorkWiseDbContext();
            return await ctx.Users.AnyAsync(u => u.Username == username);
        }
    }
}
