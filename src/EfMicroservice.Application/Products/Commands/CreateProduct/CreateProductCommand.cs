using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EfMicroservice.Application.Products.Mappings;
using EfMicroservice.Application.Products.Queries;
using EfMicroservice.Application.Shared.Repositories;
using EfMicroservice.Domain.Products;
using FluentValidation;

namespace EfMicroservice.Application.Products.Commands.CreateProduct
{
    public class CreateProductCommand : ICreateProductCommand
    {
        private readonly IProductMapper _productMapper;
        private readonly IUnitOfWork _unitOfWork;

        public CreateProductCommand(IProductMapper productMapper, IUnitOfWork unitOfWork)
        {
            _productMapper = productMapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ProductModel> ExecuteAsync(CreateProductModel productToCreate)
        {
            var product = _productMapper.Map(productToCreate);
            product.TryValidate();

            var createdProduct = await _unitOfWork.Products.AddAsync(product);

            await _unitOfWork.SaveAsync();

            return _productMapper.Map(createdProduct);
        }
    }
}