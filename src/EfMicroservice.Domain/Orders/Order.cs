using System;
using System.Collections.Generic;
using System.Text;
using EfMicroservice.Common.Persistence;
using EfMicroservice.Domain.Products;

namespace EfMicroservice.Domain.Orders
{
    public class Order : BaseEntity<int>, IVersionInfo
    {
        public Guid ProductId { get; set; }

        public Product Product { get; set; }

        public int Quantity { get; set; }

        public byte[] RowVersion { get; set; }

        public decimal TotalCost()
        {
            return Quantity * Product.Price;
        }
    }
}