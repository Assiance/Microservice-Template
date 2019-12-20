using EfMicroservice.Api.Infrastructure.Configurations;
using EfMicroservice.ExternalData.Clients;
using EfMicroservice.ExternalData.Clients.ClientConfigurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Omni.BuildingBlocks.Api.Configuration.HttpClient;
using Omni.BuildingBlocks.Http.Handlers;
using System;
using EfMicroservice.Application.Products.Clients;

namespace EfMicroservice.Api.Infrastructure.Registrations
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
                })
                .AddPolicy(policy)
                .AddHttpMessageHandler<UnsuccessfulResponseHandler>()
                .AddReAuthHandler(client);
        }
    }
}
