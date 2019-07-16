using EfMicroservice.Common.Persistence.Extensions;
using EfMicroservice.Domain.Orders;
using EfMicroservice.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfMicroservice.Persistence.Orders
{
    public class OrderTypeConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Quantity)
                .IsRequired();

            builder.HasRowVersion();
        }
    }
}
