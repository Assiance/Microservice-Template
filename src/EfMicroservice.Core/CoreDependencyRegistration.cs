using System.Reflection;
using EfMicroservice.Core.DI;
using Microsoft.Extensions.DependencyInjection;


namespace EfMicroservice.Core
{
    public static class CoreDependencyRegistration
    {
        public static IServiceCollection RegisterCoreDependencies(this IServiceCollection services)
        {
            return services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetExecutingAssembly());
        }
    }
}
