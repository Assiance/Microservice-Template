using System;
using System.Collections.Generic;
using System.Text;

namespace EfMicroservice.Application.Products.Queries
{
    public class ProductModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public byte[] RowVersion { get; set; }
    }
}
