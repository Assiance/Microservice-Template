using System;

namespace EfMicroservice.Application.Products.Queries
{
    public class ProductModel : IAuditInfoModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public byte[] RowVersion { get; set; }
    }
}