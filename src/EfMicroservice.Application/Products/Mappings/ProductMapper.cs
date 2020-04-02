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
                RowVersion = source.RowVersion,
                CreatedBy = source.CreatedBy,
                ModifiedBy = source.ModifiedBy,
                CreatedDate = source.CreatedDate,
                ModifiedDate = source.ModifiedDate
            };
        }

        public IList<ProductModel> Map(IList<Product> source)
        {
            return source?.Select(s => Map(s)).ToList();
        }

        public Product Map(CreateProductCommand source)
        {
            return new Product()
            {
                Name = source.Name,
                Price = source.Price,
                Quantity = source.Quantity
            };
        }

        public Product Map(UpdateProductCommand source)
        {
            return new Product()
            {
                Id = source.ProductId,
                Name = source.Name,
                Price = source.Price,
                Quantity = source.Quantity,
                RowVersion = source.RowVersion
            };
        }

        public UpdateProductCommand Map(ProductModel source)
        {
            return new UpdateProductCommand()
            {
                Name = source.Name,
                Price = source.Price,
                Quantity = source.Quantity,
                RowVersion = source.RowVersion
            };
        }
    }
}