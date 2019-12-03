using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Omni.BuildingBlocks.DI;

namespace EfMicroservice.ExternalData
{
    public static class ExternalDataDependencyRegistration
    {
        public static IServiceCollection RegisterExternalDataDependencies(this IServiceCollection services)
        {
            return services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetExecutingAssembly());
        }
    }
}