using System.Reflection;
using EfMicroservice.Application.Behaviors;
using EfMicroservice.Application.Orders.Commands.PlaceOrder;
using EfMicroservice.Application.Products.Commands.CreateProduct;
using EfMicroservice.Application.Products.Commands.Discontinue;
using EfMicroservice.Application.Products.Commands.UpdateProduct;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Omni.BuildingBlocks.DI;

namespace EfMicroservice.Application
{
    public static class ApplicationDependencyRegistration
    {
        public static IServiceCollection RegisterApplicationDependencies(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.AddMediatR(assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidatorBehavior<,>));

            services.AddTransient<IValidator<CreateProductCommand>, CreateProductCommandValidator>();
            services.AddTransient<IValidator<UpdateProductCommand>, UpdateProductCommandValidator>();
            services.AddTransient<IValidator<PlaceOrderCommand>, PlaceOrderModelValidator>();
            services.AddTransient<IValidator<DiscontinueProductCommand>, DiscontinueProductCommandValidator>();
            
            return services.RegisterAssemblyPublicNonGenericClasses(assembly);
        }
    }
}