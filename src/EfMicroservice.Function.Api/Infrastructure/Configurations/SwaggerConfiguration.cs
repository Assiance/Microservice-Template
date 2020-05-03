using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EfMicroservice.Function.Api.Infrastructure.Configurations
{
    public static class SwaggerConfiguration
    {
        //public static IServiceCollection AddSwagger(this IServiceCollection services)
        //{
        //    services.AddSwaggerGen(
        //        options =>
        //        {
        //            var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

        //            foreach (var description in provider.ApiVersionDescriptions)
        //            {
        //                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        //            }
        //        });

        //    return services;
        //}

        //public static IApplicationBuilder UseSwaggerUIDocs(this IApplicationBuilder app,
        //    IApiVersionDescriptionProvider provider)
        //{
        //    app.UseSwaggerUI(
        //        options =>
        //        {
        //            options.RoutePrefix = "docs";
        //            foreach (var description in provider.ApiVersionDescriptions)
        //            {
        //                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
        //                    description.GroupName.ToUpperInvariant());
        //            }
        //        });

        //    return app;
        //}

        //static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        //{
        //    var info = new OpenApiInfo()
        //    {
        //        Title = $"New Microservice API {description.ApiVersion}",
        //        Version = description.ApiVersion.ToString(),
        //        Description = "A sample microservice api.",
        //    };

        //    if (description.IsDeprecated)
        //    {
        //        info.Description += " This API version has been deprecated.";
        //    }

        //    return info;
        //}
    }
}