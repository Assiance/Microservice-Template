using System.Reflection;
using Autofac;

namespace EfMicroservice.Data
{
    public static class DataDependencyRegistration
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