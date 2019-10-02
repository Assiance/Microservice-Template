using System;
using EfMicroservice.Common.Shared;

namespace EfMicroservice.Application.Products.Queries
{
    public class ProductModel : IAuditInfoModel, IVersionInfoModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public byte[] RowVersion { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
    }
}