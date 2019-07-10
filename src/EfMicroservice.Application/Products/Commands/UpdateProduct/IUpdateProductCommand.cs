using System;
using System.Threading.Tasks;

namespace EfMicroservice.Application.Products.Commands.UpdateProduct
{
    public interface IUpdateProductCommand
    {
        Task ExecuteAsync(Guid productId, UpdateProductModel productToUpdate);
    }
}