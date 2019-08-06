using EfMicroservice.Api.Infrastructure.Authorization;
using EfMicroservice.Common.Api.Configuration.Authentication;
using Microsoft.Extensions.DependencyInjection;

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