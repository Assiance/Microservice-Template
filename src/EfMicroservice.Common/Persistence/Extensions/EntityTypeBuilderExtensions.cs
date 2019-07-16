using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EfMicroservice.Common.Persistence.Extensions
{
    public static class EntityTypeBuilderExtensions
    {
        private static readonly ValueConverter<byte[], long> Converter = new ValueConverter<byte[], long>(
            v => BitConverter.ToInt64(v, 0),
            v => BitConverter.GetBytes(v));

        public static void HasRowVersion<T>(this EntityTypeBuilder<T> builder)
            where T : class, IVersionInfo
        {
            builder
                .Property(x => x.RowVersion)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .HasConversion(Converter)
                .IsRowVersion();
        }
    }
}
