using EfMicroservice.Application.Products.Mappings;
using EfMicroservice.Application.Shared.Repositories;
using EfMicroservice.Domain.Products;
using FluentValidation;
using MediatR;
using Omni.BuildingBlocks.ExceptionHandling.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EfMicroservice.Application.Products.Commands
{
    public class DiscontinueProductCommand : IRequest
    {
        public Guid ProductId { get; set; }
    }

    public class DiscontinueProductCommandHandler : IRequestHandler<DiscontinueProductCommand>
    {
        private readonly IProductMapper _productMapper;
        private readonly IUnitOfWork _unitOfWork;

        public DiscontinueProductCommandHandler(IProductMapper productMapper, IUnitOfWork unitOfWork)
        {
            _productMapper = productMapper;
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DiscontinueProductCommand productToUpdate, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.Products.FindAsync(productToUpdate.ProductId);
            if (product == null)
            {
                throw new NotFoundException($"{nameof(Product)}");
            }

            product.SetDiscontinueStatus();

            await _unitOfWork.SaveAsync();
        }
    }

    public class DiscontinueProductCommandValidator : AbstractValidator<DiscontinueProductCommand>
    {
        public DiscontinueProductCommandValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty();
        }
    }
}