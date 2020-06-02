using FluentValidation;

namespace EfMicroservice.Domain.Products
{
    public class ProductValidator : AbstractValidator<Product>
    {
        public ProductValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0);
        }
    }
}