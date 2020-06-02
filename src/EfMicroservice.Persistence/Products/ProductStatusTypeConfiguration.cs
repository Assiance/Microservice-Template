using EfMicroservice.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfMicroservice.Persistence.Products
{
    public class ProductStatusTypeConfiguration : IEntityTypeConfiguration<ProductStatus>
    {
        public void Configure(EntityTypeBuilder<ProductStatus> builder)
        {
            builder.ToTable("product_statuses");

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
