using Microsoft.Extensions.DependencyInjection;
using Omni.BuildingBlocks.DI;
using System.Reflection;

namespace EfMicroservice.Persistence
{
    public static class PersistenceDependencyRegistration
    {
        public static IServiceCollection RegisterPersistenceDependencies(this IServiceCollection services)
        {
            return services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetExecutingAssembly());
        }
    }
}