using EfMicroservice.Application.Products.Commands.CreateProduct;
using EfMicroservice.Application.Products.Commands.UpdateProduct;
using EfMicroservice.Application.Products.Queries;
using EfMicroservice.Domain.Products;
using System.Collections.Generic;
using System.Linq;

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
                Status = source.StatusId.ToString(),
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

        public Product Map(CreateProductCommand source) => new Product(source.Name, source.Price, source.Quantity);

        public Product Map(UpdateProductCommand source)
        {
            var product = new Product(source.Name, source.Price, source.Quantity);
            product.Id = source.ProductId;
            product.RowVersion = source.RowVersion;
            
            return product;
        }

        public UpdateProductCommand Map(ProductModel source)
        {
            return new UpdateProductCommand()
            {
                ProductId = source.Id,
                Name = source.Name,
                Price = source.Price,
                Quantity = source.Quantity,
                RowVersion = source.RowVersion
            };
        }
    }
}