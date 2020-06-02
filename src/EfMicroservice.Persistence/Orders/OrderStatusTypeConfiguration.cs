using EfMicroservice.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfMicroservice.Persistence.Orders
{
    public class OrderStatusTypeConfiguration : IEntityTypeConfiguration<OrderStatus>
    {
        public void Configure(EntityTypeBuilder<OrderStatus> builder)
        {
            builder.ToTable("order_statuses");

            builder.HasKey(o => o.Id);

            builder.Property(x => x.Id)
                .HasConversion<int>();

            builder.Property(o => o.Name)
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(o => o.Description)
                .HasMaxLength(256);
        }
    }
}
