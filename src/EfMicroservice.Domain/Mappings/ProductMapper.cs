using System.Collections.Generic;
using System.Linq;
using EfMicroservice.Data.Models;
using EfMicroservice.Domain.Mappings.Interfaces;
using EfMicroservice.Domain.Model.Product;

namespace EfMicroservice.Domain.Mappings
{
    public class ProductMapper : IProductMapper
    {
        public ProductEntity Map(Product source)
        {
            return source == null
                ? null
                : new ProductEntity()
                {
                    Id = source.Id,
                    Name = source.Name,
                    Price = source.Price,
                    Quantity = source.Quantity,
                    RowVersion = source.RowVersion
                };
        }

        public Product Map(ProductEntity source)
        {
            return source == null
                ? null
                : new Product()
                {
                    Id = source.Id,
                    Name = source.Name,
                    Price = source.Price,
                    Quantity = source.Quantity,
                    RowVersion = source.RowVersion
                };
        }

        public IList<Product> Map(IList<ProductEntity> source)
        {
            return source?.Select(s => new Product()
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                Quantity = s.Quantity,
                RowVersion = s.RowVersion
            }).ToList();
        }
    }
}
