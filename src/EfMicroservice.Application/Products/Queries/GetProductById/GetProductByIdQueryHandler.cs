using EfMicroservice.Application.Products.Mappings;
using EfMicroservice.Application.Shared.Repositories;
using EfMicroservice.Domain.Products;
using MediatR;
using Omni.BuildingBlocks.ExceptionHandling.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace EfMicroservice.Application.Products.Queries.GetProductById
{
    public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductModel>
    {
        private readonly IProductMapper _productMapper;
        private readonly IUnitOfWork _unitOfWork;

        public GetProductByIdQueryHandler(IProductMapper productMapper, IUnitOfWork unitOfWork)
        {
            _productMapper = productMapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ProductModel> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.Products.FindAsync(request.ProductId);
            if (product == null)
            {
                throw new NotFoundException($"{nameof(Product)}: {request.ProductId} not found.");
            }

            return _productMapper.Map(product);
        }
    }
}