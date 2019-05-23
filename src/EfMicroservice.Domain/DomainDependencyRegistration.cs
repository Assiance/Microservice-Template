using System.Reflection;
using Autofac;

namespace EfMicroservice.Domain
{
    public static class DomainDependencyRegistration
    {
        public static void RegisterDependencies(ContainerBuilder builder)
        {
            var types = Assembly.GetExecutingAssembly();

            builder.RegisterAssemblyTypes(types)
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }
    }
}