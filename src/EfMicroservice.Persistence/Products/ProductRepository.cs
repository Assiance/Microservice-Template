using System;
using System.Threading.Tasks;
using EfMicroservice.Application.Products.Repositories;
using EfMicroservice.Common.ExceptionHandling.Exceptions;
using EfMicroservice.Common.Persistence.Extensions;
using EfMicroservice.Common.Persistence.Repositories;
using EfMicroservice.Domain.Products;
using EfMicroservice.Persistence.Contexts;
using Microsoft.Extensions.Logging;

namespace EfMicroservice.Persistence.Products
{
    public class ProductRepository : BaseRepository<Product, Guid>, IProductRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger _logger;

        public ProductRepository(ApplicationDbContext dbContext, ILoggerFactory loggerFactory)
            : base(dbContext)
        {
            _dbContext = dbContext;
            _logger = loggerFactory.CreateLogger<ProductRepository>();
        }

        public override async Task UpdateAsync(Product updatedProduct)
        {
            var retrievedProduct = await FindAsync(updatedProduct.Id);

            if (retrievedProduct == null)
            {
                throw new NotFoundException();
            }

            retrievedProduct.Name = updatedProduct.Name;
            retrievedProduct.Price = updatedProduct.Price;
            retrievedProduct.Quantity = updatedProduct.Quantity;

            _dbContext.UpdateRowVersion(retrievedProduct, updatedProduct.RowVersion);
            _dbContext.Products.Update(retrievedProduct);
        }
    }
}