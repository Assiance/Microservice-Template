using System;
using EfMicroservice.Application.Behaviors;
using MediatR;

namespace EfMicroservice.Application.Products.Commands.UpdateProduct
{
    public class UpdateProductCommand : IRequest
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public byte[] RowVersion { get; set; }
    }
}