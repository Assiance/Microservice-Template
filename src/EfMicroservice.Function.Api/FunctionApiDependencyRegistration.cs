using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Omni.BuildingBlocks.DI;

namespace EfMicroservice.Function.Api
{
    public static class FunctionApiDependencyRegistration
    {
        public static IServiceCollection RegisterFunctionApiDependencies(this IServiceCollection services)
        {
            return services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetExecutingAssembly());
        }
    }
}