using System.Threading.Tasks;
using EfMicroservice.Application.Products.Queries;

namespace EfMicroservice.Application.Products.Commands.CreateProduct
{
    public interface ICreateProductCommand
    {
        Task<ProductModel> ExecuteAsync(CreateProductModel productToCreate);
    }
}