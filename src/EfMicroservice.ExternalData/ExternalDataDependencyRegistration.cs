using Microsoft.Extensions.DependencyInjection;
using Omni.BuildingBlocks.DI;
using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EfMicroservice.ExternalData
{
    public static class ExternalDataDependencyRegistration
    {
        public static IServiceCollection RegisterExternalDataDependencies(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAssemblyPublicNonGenericClasses(assembly);
            services.RemoveDependenciesThatEndWithClient(assembly);

            return services;
        }

        private static IServiceCollection RemoveDependenciesThatEndWithClient(this IServiceCollection services, params Assembly[] assemblies)
        {
            var allPublicTypes = assemblies.SelectMany(x => x.GetExportedTypes()
                .Where(y => y.IsClass && !y.IsAbstract && !y.IsGenericType && !y.IsNested && y.Name.EndsWith("Client")));

            foreach (var classType in allPublicTypes)
            {
                var interfaces = classType.GetTypeInfo().ImplementedInterfaces
                    .Where(i => i != typeof(IDisposable) && i.IsPublic && i.Name == "I" + classType.Name);

                foreach (var infc in interfaces)
                {
                    var existingDependency = services.FirstOrDefault(x => x.ServiceType.FullName == infc.FullName);
                    if (existingDependency != null)
                    {
                        services.RemoveAll(existingDependency.ServiceType);
                    }
                }
            }

            return services;
        }
    }
}