using System;
using MediatR;

namespace EfMicroservice.Application.Products.Commands.Discontinue
{
    public class DiscontinueProductCommand : IRequest
    {
        public Guid ProductId { get; set; }
    }
}