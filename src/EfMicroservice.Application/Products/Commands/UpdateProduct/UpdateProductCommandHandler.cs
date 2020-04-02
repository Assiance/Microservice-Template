using System;
using System.Threading;
using System.Threading.Tasks;
using EfMicroservice.Application.Products.Commands.DeleteProduct;
using EfMicroservice.Application.Products.Mappings;
using EfMicroservice.Application.Shared.Repositories;
using EfMicroservice.Domain.Products;
using FluentValidation;
using MediatR;

namespace EfMicroservice.Application.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandHandler : AsyncRequestHandler<UpdateProductCommand>
    {
        private readonly IProductMapper _productMapper;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProductCommandHandler(IProductMapper productMapper, IUnitOfWork unitOfWork)
        {
            _productMapper = productMapper;
            _unitOfWork = unitOfWork;
        }

        protected override async Task Handle(UpdateProductCommand productToUpdate, CancellationToken cancellationToken)
        {
            var product = _productMapper.Map(productToUpdate);
            product.TryValidate();

            await _unitOfWork.Products.UpdateAsync(product);

            await _unitOfWork.SaveAsync();
        }
    }
}