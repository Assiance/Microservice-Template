using System.Reflection;
using EfMicroservice.Application.Behaviors;
using EfMicroservice.Application.Orders.Commands.PlaceOrder;
using EfMicroservice.Application.Products.Commands;
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
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

            // Pipeline behavior registration order: outermost first
            // LoggingBehavior → IdempotencyBehavior → ValidatorBehavior → TransientFaultBehavior → Handler
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidatorBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransientFaultBehavior<,>));

            services.AddTransient<IValidator<CreateProductCommand>, CreateProductCommandValidator>();
            services.AddTransient<IValidator<UpdateProductCommand>, UpdateProductCommandValidator>();
            services.AddTransient<IValidator<PlaceOrderCommand>, PlaceOrderModelValidator>();
            services.AddTransient<IValidator<DiscontinueProductCommand>, DiscontinueProductCommandValidator>();

            return services.RegisterAssemblyPublicNonGenericClasses(assembly);
        }
    }
}
