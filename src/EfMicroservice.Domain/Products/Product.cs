using EfMicroservice.Common.Persistence;
using EfMicroservice.Domain.Orders;
using FluentValidation;
using System;
using System.Collections.Generic;

namespace EfMicroservice.Domain.Products
{
    public class Product : BaseEntity<Guid>, IVersionInfo, IAuditInfo
    {
        public string Name { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public IEnumerable<Order> Orders { get; set; }

        public byte[] RowVersion { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }

        public void TryValidate()
        {
            var validator = new ProductValidator();
            validator.ValidateAndThrow(this);
        }
    }
}