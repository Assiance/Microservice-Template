using EfMicroservice.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EfMicroservice.Data.EntityTypeConfigurations
{
    public class ValueEntityTypeConfiguration : IEntityTypeConfiguration<ValueEntity>
    {
        public void Configure(EntityTypeBuilder<ValueEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
