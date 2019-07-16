using System.Collections.Generic;
using System.Threading.Tasks;

namespace EfMicroservice.Application.Products.Queries.GetProducts
{
    public interface IGetProductsQuery
    {
        Task<IList<ProductModel>> ExecuteAsync();
    }
}