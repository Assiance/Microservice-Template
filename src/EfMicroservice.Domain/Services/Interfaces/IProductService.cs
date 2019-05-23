using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EfMicroservice.Domain.Model.Product;

namespace EfMicroservice.Domain.Services.Interfaces
{
    public interface IProductService
    {
        Task<IList<Product>> GetProductsAsync();

        Task<Product> GetProductByIdAsync(Guid productId);

        Task<Product> CreateProductAsync(Product product);

        Task UpdateProductAsync(Product updatedProduct);

        Task DeleteProductAsync(Guid productId);
    }
}
