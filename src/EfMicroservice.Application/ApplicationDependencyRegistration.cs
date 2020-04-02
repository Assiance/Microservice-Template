using System.Reflection;
using EfMicroservice.Application.Behaviors;
using EfMicroservice.Application.Orders.Commands.PlaceOrder;
using EfMicroservice.Application.Products.Commands.CreateProduct;
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

            services.AddTransient<IValidator<CreateProductCommand>, CreateProductModelValidator>();
            services.AddTransient<IValidator<UpdateProductCommand>, UpdateProductModelValidator>();
            services.AddTransient<IValidator<PlaceOrderCommand>, PlaceOrderModelValidator>();
            
            return services.RegisterAssemblyPublicNonGenericClasses(assembly);
        }
    }
}