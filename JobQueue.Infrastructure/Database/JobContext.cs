using JobQueue.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobQueue.Infrastructure.Database;

public class JobContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Job> Jobs { get; set; }
    public DbSet<JobError> JobErrors { get; set; }
    public DbSet<RecurringJob> RecurringJobs { get; set; }
    public DbSet<DeadLetterJob> DeadLetterJobs { get; set; }
    public DbSet<Outbox> Outbox { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JobContext).Assembly);
    }
}