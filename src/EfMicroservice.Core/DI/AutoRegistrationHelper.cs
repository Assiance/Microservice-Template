using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace EfMicroservice.Core.DI
{
    public static class AutoRegistrationHelper
    {
        public static IServiceCollection RegisterAssemblyPublicNonGenericClasses(this IServiceCollection services, params Assembly[] assemblies)
        {
            var allPublicTypes = assemblies.SelectMany(x => x.GetExportedTypes()
                .Where(y => y.IsClass && !y.IsAbstract && !y.IsGenericType && !y.IsNested));

            foreach (var classType in allPublicTypes)
            {
                var interfaces = classType.GetTypeInfo().ImplementedInterfaces
                    .Where(i => i != typeof(IDisposable) && i.IsPublic && i.Name == "I" + classType.Name);

                foreach (var infc in interfaces)
                {
                    services.Add(new ServiceDescriptor(infc, classType, ServiceLifetime.Scoped));
                }
            }

            return services;
        }
    }
}
