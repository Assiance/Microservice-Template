using EfMicroservice.Persistence.Idempotency;
using EfMicroservice.Persistence.Outbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Omni.BuildingBlocks.Application.Idempotency;
using Omni.BuildingBlocks.DI;
using System.Reflection;

namespace EfMicroservice.Persistence
{
    public static class PersistenceDependencyRegistration
    {
        public static IServiceCollection RegisterPersistenceDependencies(this IServiceCollection services, IConfiguration configuration = null)
        {
            services.AddScoped<IIdempotencyStore, EfIdempotencyStore>();

            if (configuration != null)
            {
                services.Configure<OutboxOptions>(configuration.GetSection("Outbox"));
            }

            services.AddHostedService<OutboxProcessorService>();

            return services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetExecutingAssembly());
        }
    }
}
