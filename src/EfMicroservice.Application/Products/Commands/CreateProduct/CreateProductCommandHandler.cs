using EfMicroservice.Application.Products.Mappings;
using EfMicroservice.Application.Products.Queries;
using EfMicroservice.Application.Shared.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EfMicroservice.Application.Products.Commands.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductModel>
    {
        private readonly IProductMapper _productMapper;
        private readonly IUnitOfWork _unitOfWork;

        public CreateProductCommandHandler(IProductMapper productMapper, IUnitOfWork unitOfWork)
        {
            _productMapper = productMapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<ProductModel> Handle(CreateProductCommand productToCreate, CancellationToken cancellationToken)
        {
            var product = _productMapper.Map(productToCreate);
            product.TryValidate();

            var createdProduct = await _unitOfWork.Products.AddAsync(product);

            await _unitOfWork.SaveAsync();

            return _productMapper.Map(createdProduct);
        }
    }
}