using System;
using System.Threading.Tasks;

namespace EfMicroservice.Application.Products.Commands.DeleteProduct
{
    public interface IDeleteProductCommand
    {
        Task ExecuteAsync(Guid productId);
    }
}