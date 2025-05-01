using System;
using System.Collections.Generic;
using DBentities.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL;

public partial class WorkWiseDbContext : DbContext
{
    public WorkWiseDbContext()
    {
    }

    public WorkWiseDbContext(DbContextOptions<WorkWiseDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    public virtual DbSet<Task_> Tasks { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<Worker> Workers { get; set; }

    public virtual DbSet<WorkerAbsence> WorkerAbsences { get; set; }

    public virtual DbSet<WorkerAvailability> WorkerAvailabilities { get; set; }

    public virtual DbSet<WorkerSkill> WorkerSkills { get; set; }
    public virtual DbSet<TaskDependency> TaskDependencies { get; set; }

    public virtual DbSet<TaskRequiredSkill> TaskRequiredSkills { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=WorkWiseDB;Integrated Security=True;Encrypt=False;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__Schedule__C46A8A6F3E434660");

            entity.ToTable("Schedule");

            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.AssignedHours)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("assigned_hours");
            entity.Property(e => e.FinishTime)
                .HasColumnType("datetime")
                .HasColumnName("finish_time");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasDefaultValue("Scheduled")
                .HasColumnName("status");
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.WorkerId).HasColumnName("worker_id");

            entity.HasOne(d => d.Task).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("FK__Schedule__task_i__797309D9");

            entity.HasOne(d => d.Worker).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.WorkerId)
                .HasConstraintName("FK__Schedule__worker__7A672E12");
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.SkillId).HasName("PK__Skills__FBBA837929B256B3");

            entity.HasIndex(e => e.Name, "UQ__Skills__72E12F1B92439211").IsUnique();

            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Task_>(entity =>
        {
            entity.HasKey(e => e.TaskId).HasName("PK__Tasks__0492148D622FB2C2");

            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.AssignedTeamId).HasColumnName("assigned_team_id");
            entity.Property(e => e.ComplexityLevel)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("Medium")
                .HasColumnName("complexity_level");
            entity.Property(e => e.Deadline)
                .HasColumnType("datetime")
                .HasColumnName("deadline");
            entity.Property(e => e.Duration)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("duration");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.PriorityLevel).HasColumnName("priority_level");
            entity.Property(e => e.RequiredWorkers)
                .HasDefaultValue(1)
                .HasColumnName("required_workers");

            entity.HasOne(d => d.AssignedTeam).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.AssignedTeamId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Tasks__assigned___5165187F");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.TeamId).HasName("PK__Teams__F82DEDBC418084A2");

            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Worker>(entity =>
        {
            entity.HasKey(e => e.WorkerId).HasName("PK__Workers__569F8007444857C3");

            entity.Property(e => e.WorkerId).HasColumnName("worker_id");
            entity.Property(e => e.DailyHours)
                .HasColumnType("decimal(4, 2)")
                .HasColumnName("daily_hours");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("last_name");
            entity.Property(e => e.MaxWeeklyHours)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("max_weekly_hours");
            entity.Property(e => e.TeamId).HasColumnName("team_id");

            entity.HasOne(d => d.Team).WithMany(p => p.Workers)
                .HasForeignKey(d => d.TeamId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__Workers__team_id__398D8EEE");
        });

        modelBuilder.Entity<WorkerAbsence>(entity =>
        {
            entity.HasKey(e => e.AbsenceId).HasName("PK__WorkerAb__9BAC7E738E194488");

            entity.Property(e => e.AbsenceId).HasColumnName("absence_id");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Reason)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Other")
                .HasColumnName("reason");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.WorkerId).HasColumnName("worker_id");

            entity.HasOne(d => d.Worker).WithMany(p => p.WorkerAbsences)
                .HasForeignKey(d => d.WorkerId)
                .HasConstraintName("FK__WorkerAbs__worke__4222D4EF");
        });

        modelBuilder.Entity<WorkerAvailability>(entity =>
        {
            entity.HasKey(e => new { e.WorkerId, e.WorkDay }).HasName("PK__WorkerAv__CDA81BF92BFA7F51");

            entity.ToTable("WorkerAvailability");

            entity.Property(e => e.WorkerId).HasColumnName("worker_id");
            entity.Property(e => e.WorkDay)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("work_day");

            entity.HasOne(d => d.Worker).WithMany(p => p.WorkerAvailabilities)
                .HasForeignKey(d => d.WorkerId)
                .HasConstraintName("FK__WorkerAva__worke__3D5E1FD2");
        });

        modelBuilder.Entity<WorkerSkill>(entity =>
        {
            entity.HasKey(e => new { e.WorkerId, e.SkillId }).HasName("PK__WorkerSk__D9242830C54B7BEE");

            entity.Property(e => e.WorkerId).HasColumnName("worker_id");
            entity.Property(e => e.SkillId).HasColumnName("skill_id");
            entity.Property(e => e.ProficiencyLevel)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasDefaultValue("Intermediate")
                .HasColumnName("proficiency_level");

            entity.HasOne(d => d.Skill).WithMany(p => p.WorkerSkills)
                .HasForeignKey(d => d.SkillId)
                .HasConstraintName("FK__WorkerSki__skill__4AB81AF0");

            entity.HasOne(d => d.Worker).WithMany(p => p.WorkerSkills)
                .HasForeignKey(d => d.WorkerId)
                .HasConstraintName("FK__WorkerSki__worke__49C3F6B7");
        });

        modelBuilder.Entity<TaskDependency>(entity =>
        {
            entity.HasKey(e => new { e.TaskId, e.DependentTaskId }).HasName("PK__TaskDepe__5256FCCE5FDD3BD1");
            entity.ToTable("TaskDependencies");

            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.DependentTaskId).HasColumnName("dependent_task_id");

            // Configure the relationship with Task
            entity.HasOne(d => d.Task)
                .WithMany(p => p.TaskDependenciesAsParent)  // Update property name to match your Task_ class
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("FK__TaskDepen__task___01142BA1");

            entity.HasOne(d => d.DependentTask)
                .WithMany(p => p.TaskDependenciesAsDependent)  // Update property name to match your Task_ class
                .HasForeignKey(d => d.DependentTaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TaskDepen__depen__02084FDA");
        });

        modelBuilder.Entity<TaskRequiredSkill>(entity =>
        {
            entity.HasKey(e => new { e.TaskId, e.SkillId }).HasName("PK__TaskRequ__8B29BCBA2E77089F");
            entity.ToTable("TaskRequiredSkills");

            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.SkillId).HasColumnName("skill_id");

            //    // Configure the relationships
            //    entity.HasOne<Task_>()
            //        .WithMany(p => p.TaskRequiredSkills)
            //        .HasForeignKey(d => d.TaskId)
            //        .HasConstraintName("FK__TaskRequi__task___73BA3083");

            //    entity.HasOne<Skill>()
            //        .WithMany(p => p.TaskRequiredSkills)
            //        .HasForeignKey(d => d.SkillId)
            //        .HasConstraintName("FK__TaskRequi__skill__74AE54BC");
            //
            entity.HasOne(d => d.Task)  // Use the navigation property name
        .WithMany(p => p.TaskRequiredSkills)
        .HasForeignKey(d => d.TaskId)
        .HasConstraintName("FK__TaskRequi__task___73BA3083");

            entity.HasOne(d => d.Skill)  // Use the navigation property name
                .WithMany(p => p.TaskRequiredSkills)
                .HasForeignKey(d => d.SkillId)
                .HasConstraintName("FK__TaskRequi__skill__74AE54BC");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
