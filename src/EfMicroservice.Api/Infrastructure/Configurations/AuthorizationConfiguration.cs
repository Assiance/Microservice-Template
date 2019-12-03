using EfMicroservice.Api.Infrastructure.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Omni.BuildingBlocks.Api.Configuration.Authentication;

namespace EfMicroservice.Api.Infrastructure.Configurations
{
    public static class AuthorizationConfiguration
    {
        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services,
            JwtConfiguration authConfig)
        {
            // Todo: EF - Change
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Permissions.ReadMessages,
                    policy => policy.Requirements.Add(new HasScopeRequirement(Permissions.ReadMessages,
                        authConfig.ValidIssuer)));
            });

            return services;
        }
    }
}