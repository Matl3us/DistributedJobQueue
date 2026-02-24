using JobQueue.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobQueue.Infrastructure.Database;

public class JobContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Job> Jobs { get; set; }
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

        modelBuilder.Entity<DeadLetterJob>()
            .HasOne(d => d.Job)
            .WithOne(j => j.DeadLetterJob)
            .HasForeignKey<DeadLetterJob>(d => d.JobId);

        modelBuilder.Entity<DeadLetterJob>()
            .Property(d => d.Reason)
            .HasMaxLength(512);
    }
}