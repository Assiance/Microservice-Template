using FluentValidation;

namespace EfMicroservice.Application.Products.Commands.Discontinue
{
    public class DiscontinueProductCommandValidator : AbstractValidator<DiscontinueProductCommand>
    {
        public DiscontinueProductCommandValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty();
        }
    }
}
