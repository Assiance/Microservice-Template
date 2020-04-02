using System;
using MediatR;

namespace EfMicroservice.Application.Products.Queries.GetProductById
{
    public class GetProductByIdQuery : IRequest<ProductModel>
    {
        public GetProductByIdQuery(Guid productId)
        {
            ProductId = productId;
        }

        public Guid ProductId { get; set; }
    }
}
