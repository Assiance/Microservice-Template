using FluentValidation;

namespace EfMicroservice.Application.Orders.Commands.PlaceOrder
{
    public class PlaceOrderModelValidator : AbstractValidator<PlaceOrderModel>
    {
        public PlaceOrderModelValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty();
            RuleFor(x => x.Quantity)
                .NotEmpty()
                .GreaterThan(0);
        }
    }
}
