using EfMicroservice.Application.Products.Mappings;
using EfMicroservice.Application.Shared.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using EfMicroservice.Domain.Products;
using Omni.BuildingBlocks.ExceptionHandling.Exceptions;

namespace EfMicroservice.Application.Products.Commands.Discontinue
{
    public class DiscontinueProductCommandHandler : AsyncRequestHandler<DiscontinueProductCommand>
    {
        private readonly IProductMapper _productMapper;
        private readonly IUnitOfWork _unitOfWork;

        public DiscontinueProductCommandHandler(IProductMapper productMapper, IUnitOfWork unitOfWork)
        {
            _productMapper = productMapper;
            _unitOfWork = unitOfWork;
        }

        protected override async Task Handle(DiscontinueProductCommand productToUpdate, CancellationToken cancellationToken)
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
}