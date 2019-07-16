using System;
using System.Collections.Generic;
using System.Linq;
using EfMicroservice.Application.Products.Commands.CreateProduct;
using EfMicroservice.Application.Products.Commands.UpdateProduct;
using EfMicroservice.Application.Products.Queries;
using EfMicroservice.Domain.Products;

namespace EfMicroservice.Application.Products.Mappings
{
    public class ProductMapper : IProductMapper
    {
        public ProductModel Map(Product source)
        {
            return new ProductModel()
            {
                Id = source.Id,
                Name = source.Name,
                Price = source.Price,
                Quantity = source.Quantity,
                RowVersion = source.RowVersion
            };
        }

        public IList<ProductModel> Map(IList<Product> source)
        {
            return source?.Select(s => Map(s)).ToList();
        }

        public Product Map(CreateProductModel source)
        {
            return new Product()
            {
                Name = source.Name,
                Price = source.Price,
                Quantity = source.Quantity
            };
        }

        public Product Map(Guid productId, UpdateProductModel source)
        {
            return new Product()
            {
                Id = productId,
                Name = source.Name,
                Price = source.Price,
                Quantity = source.Quantity,
                RowVersion = source.RowVersion
            };
        }
    }
}
