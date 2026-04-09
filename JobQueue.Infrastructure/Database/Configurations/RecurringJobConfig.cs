using JobQueue.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobQueue.Infrastructure.Database.Configurations;

public class RecurringJobConfiguration : IEntityTypeConfiguration<RecurringJob>
{
    public void Configure(EntityTypeBuilder<RecurringJob> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).IsRequired().HasMaxLength(512);
        builder.Property(r => r.Type).IsRequired();
        builder.Property(r => r.Payload).HasColumnType("jsonb");
        builder.Property(r => r.CronExpression).IsRequired().HasMaxLength(256);
    }
}