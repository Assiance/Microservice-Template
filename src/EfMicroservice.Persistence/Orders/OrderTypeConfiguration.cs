using EfMicroservice.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Omni.BuildingBlocks.Persistence.Extensions;

namespace EfMicroservice.Persistence.Orders
{
    public class OrderTypeConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("orders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Quantity)
                .IsRequired();

            builder.HasOne(x => x.Status)
                .WithMany()
                .IsRequired()
                .HasForeignKey(x => x.StatusId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasRowVersion();
        }
    }
}