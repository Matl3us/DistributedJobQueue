using JobQueue.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobQueue.Infrastructure.Database.Configurations;

public class JobConfig : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasKey(j => j.Id);

        builder.Property(j => j.Type).IsRequired();
        builder.Property(j => j.Status).IsRequired();
        builder.Property(j => j.Priority).IsRequired();
        builder.Property(j => j.Payload).HasColumnType("jsonb");
        builder.Property(j => j.Result).HasColumnType("jsonb");
        builder.Property(j => j.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(j => j.UpdatedAt).HasDefaultValueSql("now()");
        builder.Property(j => j.RetryCount).HasDefaultValue(0);

        builder.HasOne(j => j.RecurringJob)
            .WithMany(r => r.Jobs)
            .HasForeignKey(j => j.RecurringJobId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(j => j.DeadLetterJob)
            .WithOne(d => d.Job)
            .HasForeignKey<DeadLetterJob>(d => d.JobId);

        builder.HasMany(j => j.Errors)
            .WithOne(e => e.Job)
            .HasForeignKey(e => e.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(j => j.Outbox)
            .WithOne(o => o.Job)
            .HasForeignKey<Outbox>(o => o.JobId);
    }
}