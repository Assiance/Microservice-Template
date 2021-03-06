﻿using EfMicroservice.Application.Products.Mappings;
using EfMicroservice.Application.Shared.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

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

            await _unitOfWork.Products.UpdateAsync(product);

            await _unitOfWork.SaveAsync();
        }
    }
}