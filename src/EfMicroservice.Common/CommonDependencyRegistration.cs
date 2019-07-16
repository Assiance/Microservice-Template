using System.Reflection;
using EfMicroservice.Common.DI;
using Microsoft.Extensions.DependencyInjection;

namespace EfMicroservice.Common
{
    public static class CommonDependencyRegistration
    {
        public static IServiceCollection RegisterCommonDependencies(this IServiceCollection services)
        {
            return services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetExecutingAssembly());
        }
    }
}
