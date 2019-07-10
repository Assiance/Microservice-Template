using System;
using EfMicroservice.Common.Persistence;

namespace EfMicroservice.Domain.Products
{
    public class Product : BaseEntity<Guid>, IVersionInfo
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
