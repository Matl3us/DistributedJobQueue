using JobQueue.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobQueue.Infrastructure.Database.Configurations;

public class DeadLetterJobConfiguration : IEntityTypeConfiguration<DeadLetterJob>
{
    public void Configure(EntityTypeBuilder<DeadLetterJob> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Reason).IsRequired().HasMaxLength(512);
        builder.Property(d => d.CreatedAt).HasDefaultValueSql("now()");
    }
}