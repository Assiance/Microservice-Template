using System.Reflection;
using Autofac;

namespace EfMicroservice.Api
{
    public static class ApiDependencyRegistration
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