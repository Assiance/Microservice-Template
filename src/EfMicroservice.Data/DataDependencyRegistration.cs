using System.Reflection;
using EfMicroservice.Core.DI;
using Microsoft.Extensions.DependencyInjection;

namespace EfMicroservice.Data
{
    public static class DataDependencyRegistration
    {
        public static IServiceCollection RegisterDataDependencies(this IServiceCollection services)
        {
            return services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetExecutingAssembly());
        }
    }
}