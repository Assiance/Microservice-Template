using MediatR;
using System;
using EfMicroservice.Application.Behaviors;

namespace EfMicroservice.Application.Products.Commands.DeleteProduct
{
    public class DeleteProductCommand : IRequest
    {
        public Guid ProductId { get; set; }
    }
}
