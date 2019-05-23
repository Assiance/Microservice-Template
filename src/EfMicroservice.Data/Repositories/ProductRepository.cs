using System;
using System.Threading.Tasks;
using EfMicroservice.Core.Data.Extensions;
using EfMicroservice.Core.Data.Repositories;
using EfMicroservice.Core.ExceptionHandling.Exceptions;
using EfMicroservice.Data.Contexts;
using EfMicroservice.Data.Models;
using EfMicroservice.Data.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace EfMicroservice.Data.Repositories
{
    public class ProductRepository : BaseRepository<ProductEntity, Guid>, IProductRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger _logger;

        public ProductRepository(ApplicationDbContext dbContext, ILoggerFactory loggerFactory)
            : base(dbContext)
        {
            _dbContext = dbContext;
            _logger = loggerFactory.CreateLogger<ProductRepository>();
        }

        public override async Task UpdateAsync(ProductEntity updatedProduct)
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