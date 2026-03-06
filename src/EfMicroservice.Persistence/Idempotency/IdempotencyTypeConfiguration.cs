using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfMicroservice.Persistence.Idempotency
{
    public class IdempotencyTypeConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
    {
        public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
        {
            builder.ToTable("idempotency_records");
            builder.HasKey(x => x.Key);
            builder.Property(x => x.RequestType).IsRequired().HasMaxLength(500);
            builder.Property(x => x.Response).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
        }
    }
}
