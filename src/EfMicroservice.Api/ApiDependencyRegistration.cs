using System.Reflection;
using EfMicroservice.Common.DI;
using Microsoft.Extensions.DependencyInjection;

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