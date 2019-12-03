using EfMicroservice.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Omni.BuildingBlocks.Persistence.Extensions;

namespace EfMicroservice.Persistence.Products
{
    public class ProductTypeConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.Price)
                .IsRequired();

            builder.Property(x => x.Quantity)
                .IsRequired();

            builder.HasMany(x => x.Orders)
                .WithOne(x => x.Product)
                .IsRequired()
                .HasForeignKey(x => x.ProductId);

            builder.HasAuditInfo();
            builder.HasRowVersion();
        }
    }
}