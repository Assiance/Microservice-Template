using System.Collections.Generic;
using EfMicroservice.Data.Models;
using EfMicroservice.Domain.Model.Product;

namespace EfMicroservice.Domain.Mappings.Interfaces
{
    public interface IProductMapper
    {
        ProductEntity Map(Product source);

        Product Map(ProductEntity source);

        IList<Product> Map(IList<ProductEntity> source);
    }
}