using JobQueue.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobQueue.Infrastructure.Database;

public class JobContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Job> Jobs { get; set; }
    public DbSet<RecurringJob> RecurringJobs { get; set; }
    public DbSet<DeadLetterJob> DeadLetterJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Job>()
            .Property(j => j.Type)
            .IsRequired();

        modelBuilder.Entity<Job>()
            .Property(j => j.Status)
            .IsRequired();

        modelBuilder.Entity<Job>()
            .Property(j => j.Priority)
            .IsRequired();

        modelBuilder.Entity<Job>()
            .Property(j => j.Payload)
            .HasColumnType("jsonb")
            .HasMaxLength(4096);

        modelBuilder.Entity<Job>()
            .Property(j => j.CreatedAt)
            .HasDefaultValueSql("now()");

        modelBuilder.Entity<Job>()
            .Property(j => j.UpdatedAt)
            .HasDefaultValueSql("now()");

        modelBuilder.Entity<Job>()
            .Property(j => j.Result)
            .HasColumnType("jsonb")
            .HasMaxLength(4096);

        modelBuilder.Entity<Job>()
            .Property(j => j.ErrorMessages)
            .HasMaxLength(2048);

        modelBuilder.Entity<Job>()
            .Property(j => j.RetryCount)
            .HasColumnType("smallint")
            .HasDefaultValue(0);

        modelBuilder.Entity<Job>()
            .Property(j => j.NextRetryAt)
            .HasDefaultValueSql("now()");

        modelBuilder.Entity<RecurringJob>()
            .Property(r => r.Name)
            .HasMaxLength(512)
            .IsRequired();

        modelBuilder.Entity<RecurringJob>()
            .Property(r => r.Type)
            .IsRequired();

        modelBuilder.Entity<RecurringJob>()
            .Property(r => r.Payload)
            .HasColumnType("jsonb")
            .HasMaxLength(4096);

        modelBuilder.Entity<RecurringJob>()
            .Property(r => r.CronExpression)
            .HasMaxLength(256)
            .IsRequired();

        modelBuilder.Entity<RecurringJob>()
            .Property(r => r.NextRun)
            .IsRequired();

        modelBuilder.Entity<RecurringJob>()
            .HasMany(r => r.Jobs)
            .WithOne(j => j.RecurringJob)
            .HasForeignKey(j => j.RecurringJobId);

        modelBuilder.Entity<DeadLetterJob>()
            .HasOne(d => d.Job)
            .WithOne(j => j.DeadLetterJob)
            .HasForeignKey<DeadLetterJob>(d => d.JobId);

        modelBuilder.Entity<DeadLetterJob>()
            .Property(d => d.Reason)
            .HasMaxLength(512);
    }
}