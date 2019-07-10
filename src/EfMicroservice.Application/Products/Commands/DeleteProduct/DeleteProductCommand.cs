using System;
using System.Threading.Tasks;
using EfMicroservice.Application.Products.Mappings;
using EfMicroservice.Application.Shared.Repositories;

namespace EfMicroservice.Application.Products.Commands.DeleteProduct
{
    public class DeleteProductCommand : IDeleteProductCommand
    {
        private readonly IProductMapper _productMapper;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteProductCommand(IProductMapper productMapper, IUnitOfWork unitOfWork)
        {
            _productMapper = productMapper;
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteAsync(Guid productId)
        {
            await _unitOfWork.Products.RemoveAsync(productId);

            await _unitOfWork.SaveAsync();
        }
    }
}
