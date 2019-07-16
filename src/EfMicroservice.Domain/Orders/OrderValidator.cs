using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace EfMicroservice.Domain.Orders
{
    public class OrderValidator : AbstractValidator<Order>
    {
        public OrderValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThan(0);
        }
    }
}
