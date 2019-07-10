using System.Reflection;
using EfMicroservice.Common.DI;
using Microsoft.Extensions.DependencyInjection;

namespace EfMicroservice.Application
{
    public static class DomainDependencyRegistration
    {
        public static IServiceCollection RegisterDomainDependencies(this IServiceCollection services)
        {
            return services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetExecutingAssembly());
        }
    }
}