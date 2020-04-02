using FluentValidation;

namespace EfMicroservice.Application.Products.Commands.CreateProduct
{
    public class CreateProductModelValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductModelValidator()
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
