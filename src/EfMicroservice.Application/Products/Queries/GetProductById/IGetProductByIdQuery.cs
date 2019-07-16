using System;
using System.Threading.Tasks;

namespace EfMicroservice.Application.Products.Queries.GetProductById
{
    public interface IGetProductByIdQuery
    {
        Task<ProductModel> ExecuteAsync(Guid productId);
    }
}