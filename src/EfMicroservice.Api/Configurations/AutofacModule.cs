using Autofac;
using EfMicroservice.Domain;
using EfMicroservice.Data;

namespace EfMicroservice.Api.Configurations
{
    public class AutofacModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            ApiDependencyRegistration.RegisterDependencies(builder);
            DomainDependencyRegistration.RegisterDependencies(builder);
            DataDependencyRegistration.RegisterDependencies(builder);
            CoreDependencyRegistration.RegisterDependencies(builder);
        }
    }
}