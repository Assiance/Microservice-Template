using EfMicroservice.Application.Orders.Commands.PlaceOrder;
using EfMicroservice.Application.Products.Commands.CreateProduct;
using EfMicroservice.Application.Products.Commands.UpdateProduct;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EfMicroservice.Api.Infrastructure.Configurations
{
    public static class ValidationConfiguration
    {
        public static IServiceCollection RegisterRequestValidation(this IServiceCollection services)
        {
            services.AddTransient<IValidator<CreateProductModel>, CreateProductModelValidator>();
            services.AddTransient<IValidator<UpdateProductModel>, UpdateProductModelValidator>();
            services.AddTransient<IValidator<PlaceOrderModel>, PlaceOrderModelValidator>();

            return services;
        }
    }
}
