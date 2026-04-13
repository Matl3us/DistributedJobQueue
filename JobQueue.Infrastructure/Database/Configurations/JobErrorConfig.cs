using JobQueue.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobQueue.Infrastructure.Database.Configurations;

public class JobErrorConfiguration : IEntityTypeConfiguration<JobError>
{
    public void Configure(EntityTypeBuilder<JobError> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ErrorMessage).IsRequired().HasMaxLength(2048);
        builder.Property(e => e.AttemptNumber).IsRequired();
        builder.Property(e => e.OccurredAt).HasDefaultValueSql("now()");
    }
}