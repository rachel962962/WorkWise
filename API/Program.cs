using System.Reflection;
using BLL;  // Import your business logic layer
using DAL;
using IBLL; // Import interfaces
using IDAL; // Import interfaces

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ? Register BLL and DAL dependencies
            builder.Services.AddScoped<ITaskBLL, TaskBLL>();
            builder.Services.AddScoped<ITask_Dal, Task_Dal>();
            builder.Services.AddScoped<ITeamBLL, TeamBLL>();
            builder.Services.AddScoped<ITeamDAL, TeamDAL>();
            builder.Services.AddScoped<ISkillBLL, SkillBLL>();
            builder.Services.AddScoped<ISkillDAL, SkillDAL>();
            builder.Services.AddScoped<IWorkerBLL, WorkerBLL>();
            builder.Services.AddScoped<IWorkerDAL, WorkerDAL>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
