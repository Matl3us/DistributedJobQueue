using JobQueue.Core.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobQueue.Infrastructure.Database.Configurations;

public class OutboxConfiguration : IEntityTypeConfiguration<Outbox>
{
    public void Configure(EntityTypeBuilder<Outbox> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Payload).IsRequired().HasColumnType("jsonb");
        builder.Property(o => o.Published).HasDefaultValue(false);
        builder.Property(o => o.CreatedAt).HasDefaultValueSql("now()");
    }
}