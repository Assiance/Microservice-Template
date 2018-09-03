using System;
using System.Reflection;
using Autofac;

namespace EfMicroservice.Core
{
    public class CoreDependencyRegistration
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
