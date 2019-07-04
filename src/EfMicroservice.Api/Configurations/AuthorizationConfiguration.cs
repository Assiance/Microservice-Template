using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EfMicroservice.Api.Authorization;
using EfMicroservice.Core.Api.Configuration.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace EfMicroservice.Api.Configurations
{
    public static class AuthorizationConfiguration
    {
        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services, JwtConfiguration authConfig)
        {
            // Todo: EF - Change
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Permissions.ReadMessages, policy => policy.Requirements.Add(new HasScopeRequirement(Permissions.ReadMessages, authConfig.ValidIssuer)));
            });

            return services;
        }
    }
}
