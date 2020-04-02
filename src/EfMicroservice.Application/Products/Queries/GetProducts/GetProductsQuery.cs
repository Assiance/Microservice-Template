using MediatR;
using System.Collections.Generic;

namespace EfMicroservice.Application.Products.Queries.GetProducts
{
    public class GetProductsQuery : IRequest<IList<ProductModel>>
    {
    }
}
