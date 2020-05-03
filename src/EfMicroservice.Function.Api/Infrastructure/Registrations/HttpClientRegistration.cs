using System;
using EfMicroservice.Application.Products.Clients;
using EfMicroservice.ExternalData.Clients;
using EfMicroservice.ExternalData.Clients.ClientConfigurations;
using EfMicroservice.Function.Api.Infrastructure.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Omni.BuildingBlocks.Api.Configuration.HttpClient.Models;
using Omni.BuildingBlocks.Http.Handlers;

namespace EfMicroservice.Function.Api.Infrastructure.Registrations
{
    public static class HttpClientRegistration
    {
        public static IHttpClientBuilder AddGitHaubClient(this IServiceCollection services, IConfiguration configuration)
        {
            var policy = configuration.GetSection("DefaultPolicy").Get<HttpClientPolicy>();

            var clientSection = configuration.GetSection("GitHaubClient");
            services.Configure<GitHaubConfiguration>(clientSection);
            var client = clientSection.Get<GitHaubConfiguration>();

            return services.AddHttpClient<IGitHaubClient, GitHaubClient>(c =>
                {
                    c.BaseAddress = new Uri(client.BaseUrl);
                });
        }
    }
}
