using System.Reflection;
using EfMicroservice.Common.DI;
using Microsoft.Extensions.DependencyInjection;

namespace EfMicroservice.Application
{
    public static class ApplicationDependencyRegistration
    {
        public static IServiceCollection RegisterApplicationDependencies(this IServiceCollection services)
        {
            return services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetExecutingAssembly());
        }
    }
}