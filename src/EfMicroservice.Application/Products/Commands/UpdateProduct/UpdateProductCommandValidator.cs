﻿using EfMicroservice.Application.Products.Commands.CreateProduct;
using FluentValidation;

namespace EfMicroservice.Application.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty();
            RuleFor(x => x.Quantity)
                .NotEmpty()
                .GreaterThan(0);
            RuleFor(x => x.Price)
                .NotEmpty()
                .GreaterThan(0);
            RuleFor(x => x.RowVersion)
                .NotEmpty();
        }
    }
}
