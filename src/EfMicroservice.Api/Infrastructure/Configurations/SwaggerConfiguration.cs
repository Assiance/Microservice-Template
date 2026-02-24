using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;

namespace EfMicroservice.Api.Infrastructure.Configurations
{
    public static class SwaggerConfiguration
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

            foreach (var description in provider.ApiVersionDescriptions)
            {
                services.AddOpenApi(description.GroupName);
            }

            return services;
        }
    }
}
