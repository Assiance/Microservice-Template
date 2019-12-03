using Microsoft.Extensions.DependencyInjection;
using Omni.BuildingBlocks.DI;
using System.Reflection;

namespace EfMicroservice.Api
{
    public static class ApiDependencyRegistration
    {
        public static IServiceCollection RegisterApiDependencies(this IServiceCollection services)
        {
            return services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetExecutingAssembly());
        }
    }
}