using EfMicroservice.Application.Behaviors;
using EfMicroservice.Application.Products.Queries;
using MediatR;

namespace EfMicroservice.Application.Products.Commands.CreateProduct
{
    public class CreateProductCommand : IRequest<ProductModel>
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}