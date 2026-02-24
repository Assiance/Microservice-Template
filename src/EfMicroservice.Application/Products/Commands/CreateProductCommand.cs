using EfMicroservice.Application.Products.Mappings;
using EfMicroservice.Application.Products.Models;
using EfMicroservice.Application.Shared.Repositories;
using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EfMicroservice.Application.Products.Commands
{
    public class CreateProductCommand : IRequest<ProductModel>
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

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

            var createdProduct = await _unitOfWork.Products.AddAsync(product);

            await _unitOfWork.SaveAsync();

            return _productMapper.Map(createdProduct);
        }
    }

    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty();
            RuleFor(x => x.Quantity)
                .NotEmpty()
                .GreaterThan(0);
            RuleFor(x => x.Price)
                .NotEmpty()
                .GreaterThan(0);
        }
    }
}