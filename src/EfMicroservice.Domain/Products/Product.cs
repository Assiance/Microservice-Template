using System;
using System.Collections.Generic;
using EfMicroservice.Common.Persistence;
using EfMicroservice.Domain.Orders;

namespace EfMicroservice.Domain.Products
{
    public class Product : BaseEntity<Guid>, IVersionInfo
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public IEnumerable<Order> Orders { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
