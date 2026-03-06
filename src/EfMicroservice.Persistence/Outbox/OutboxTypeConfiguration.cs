using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfMicroservice.Persistence.Outbox
{
    public class OutboxTypeConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("outbox_messages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();

            builder.Property(x => x.Type)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.OccurredAt)
                .IsRequired();

            builder.Property(x => x.ProcessedAt);
            builder.Property(x => x.Error).HasMaxLength(2000);
            builder.Property(x => x.RetryCount).HasDefaultValue(0);

            builder.HasIndex(x => new { x.ProcessedAt, x.OccurredAt });
        }
    }
}
