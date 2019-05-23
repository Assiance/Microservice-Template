using System;
using EfMicroservice.Core.Data;

namespace EfMicroservice.Data.Models
{
    public class ProductEntity : BaseEntity<Guid>, IVersionInfo
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
