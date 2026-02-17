using JobQueue.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobQueue.Infrastructure.Database;

public class JobContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Job> Jobs { get; set; }

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
            .Property(j => j.Result)
            .HasColumnType("jsonb")
            .HasMaxLength(4096);
    }
}