using System;
using System.Threading.Tasks;
using EfMicroservice.Application.Products.Mappings;
using EfMicroservice.Application.Shared.Repositories;
using EfMicroservice.Domain.Products;
using FluentValidation;

namespace EfMicroservice.Application.Products.Commands.UpdateProduct
{
    public class UpdateProductCommand : IUpdateProductCommand
    {
        private readonly IProductMapper _productMapper;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProductCommand(IProductMapper productMapper, IUnitOfWork unitOfWork)
        {
            _productMapper = productMapper;
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteAsync(Guid productId, UpdateProductModel productToUpdate)
        {
            var product = _productMapper.Map(productId, productToUpdate);
            var validator = new ProductValidator();

            validator.ValidateAndThrow(product);
            await _unitOfWork.Products.UpdateAsync(product);

            await _unitOfWork.SaveAsync();
        }
    }
}
