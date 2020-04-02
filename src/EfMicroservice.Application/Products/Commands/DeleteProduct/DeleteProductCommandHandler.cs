using EfMicroservice.Application.Products.Mappings;
using EfMicroservice.Application.Shared.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EfMicroservice.Application.Products.Commands.DeleteProduct
{
    public class DeleteProductCommandHandler : AsyncRequestHandler<DeleteProductCommand>
    {
        private readonly IProductMapper _productMapper;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteProductCommandHandler(IProductMapper productMapper, IUnitOfWork unitOfWork)
        {
            _productMapper = productMapper;
            _unitOfWork = unitOfWork;
        }

        protected override async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.Products.FindAsync(request.ProductId);

            _unitOfWork.Products.Remove(product);
            await _unitOfWork.SaveAsync();
        }
    }
}