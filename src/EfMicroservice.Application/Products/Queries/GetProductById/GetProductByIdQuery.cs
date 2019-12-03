﻿using EfMicroservice.Application.Products.Mappings;
using EfMicroservice.Application.Shared.Repositories;
using System;
using System.Threading.Tasks;
using EfMicroservice.Domain.Products;
using Omni.BuildingBlocks.ExceptionHandling.Exceptions;

namespace EfMicroservice.Application.Products.Queries.GetProductById
{
    public class GetProductByIdQuery : IGetProductByIdQuery
    {
        private readonly IProductMapper _productMapper;
        private readonly IUnitOfWork _unitOfWork;

        public GetProductByIdQuery(IProductMapper productMapper, IUnitOfWork unitOfWork)
        {
            _productMapper = productMapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ProductModel> ExecuteAsync(Guid productId)
        {
            var product = await _unitOfWork.Products.FindAsync(productId);
            if (product == null)
            {
                throw new NotFoundException($"{nameof(Product)}: {productId} not found.");
            }

            return _productMapper.Map(product);
        }
    }
}