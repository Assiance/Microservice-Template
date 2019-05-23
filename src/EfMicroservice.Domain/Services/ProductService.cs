using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EfMicroservice.Data.Repositories.Interfaces;
using EfMicroservice.Domain.Mappings.Interfaces;
using EfMicroservice.Domain.Model.Product;
using EfMicroservice.Domain.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EfMicroservice.Domain.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductMapper _productMapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public ProductService(IProductMapper productMapper, IUnitOfWork unitOfWork, ILoggerFactory loggerFactory)
        {
            _productMapper = productMapper;
            _unitOfWork = unitOfWork;
            _logger = loggerFactory.CreateLogger<ProductService>();
        }

        public async Task<IList<Product>> GetProductsAsync()
        {
            var products = await _unitOfWork.Products.Queryable.ToListAsync();
            return _productMapper.Map(products);
        }

        public async Task<Product> GetProductByIdAsync(Guid productId)
        {
            var product = await _unitOfWork.Products.FindAsync(productId);
            return _productMapper.Map(product);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            var validator = new ProductValidator();

            validator.ValidateAndThrow(product);

            var productEntity = _productMapper.Map(product);
            var createdProduct = await _unitOfWork.Products.AddAsync(productEntity);

            await _unitOfWork.SaveAsync();

            return _productMapper.Map(createdProduct);
        }

        public async Task UpdateProductAsync(Product updatedProduct)
        {
            var productEntity = _productMapper.Map(updatedProduct);
            await _unitOfWork.Products.UpdateAsync(productEntity);

            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteProductAsync(Guid productId)
        {
            await _unitOfWork.Products.RemoveAsync(productId);

            await _unitOfWork.SaveAsync();
        }

        public async Task<Product> TransactionalCreateProductAsync(Product product)
        {
            using (var transaction = await _unitOfWork.BeginTransactionAsync())
            {
                var productEntity = _productMapper.Map(product);
                var createdProduct = await _unitOfWork.Products.AddAsync(productEntity);
                await _unitOfWork.SaveAsync();

                transaction.Commit();
                return _productMapper.Map(createdProduct);
            }
        }
    }
}
