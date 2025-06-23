using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BLL;  
using DAL;
using IBLL; 
using IDAL;
using BLL.Auth;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<ITaskBLL, TaskBLL>();
            builder.Services.AddScoped<ITask_Dal, Task_Dal>();
            builder.Services.AddScoped<ITeamBLL, TeamBLL>();
            builder.Services.AddScoped<ITeamDAL, TeamDAL>();
            builder.Services.AddScoped<ISkillBLL, SkillBLL>();
            builder.Services.AddScoped<ISkillDAL, SkillDAL>();
            builder.Services.AddScoped<IWorkerBLL, WorkerBLL>();
            builder.Services.AddScoped<IWorkerDAL, WorkerDAL>();
            builder.Services.AddScoped<IScheduleBLL, ScheduleBLL>();
            builder.Services.AddScoped<IScheduleDAL, ScheduleDAL>();
            builder.Services.AddScoped<IWorkerSkillBLL, WorkerSkillBLL>();
            builder.Services.AddScoped<IWorkerSkillDAL, WorkerSkillDAL>();
            builder.Services.AddScoped<IUserBLL, UserBLL>();
            builder.Services.AddScoped<IUserDAL, UserDAL>();
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            builder.Services.AddScoped<IAlgorithmManager, AlgorithmManager>();
            builder.Services.AddScoped<IFinalSchedule, FinalSchedule>();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
            builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var config = builder.Configuration;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = config["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = config["JwtSettings:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config["JwtSettings:SecretKey"])
            ),
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)

        };
    });
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
