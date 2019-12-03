using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Omni.BuildingBlocks.Api.Configuration.Authentication;
using Omni.BuildingBlocks.Authentication;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EfMicroservice.Api.Infrastructure.Configurations
{
    public static class AuthenticationConfiguration
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services,
            JwtConfiguration authConfig)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = authConfig.ValidIssuer,
                    ValidAudience = authConfig.ValidAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfig.IssuerSigningKey))
                };
            });

            return services;
        }

        public static IServiceCollection AddAccessTokenProvider(this IServiceCollection services)
        {
            var intervals = new List<int>() { 100, 500 };
            var readTimes = intervals.Select(ms => TimeSpan.FromMilliseconds(ms));

            services.AddHttpClient<IAccessTokenProvider, AccessTokenProvider>()
                .AddPolicyHandler(re => HttpPolicyExtensions.HandleTransientHttpError()
                    .WaitAndRetryAsync(readTimes,
                        (result, timespan, retryCount, context) =>
                        {
                            var request = result.Result.RequestMessage;
                            Log.Logger.Warning(
                                $"{context.PolicyKey}: refresh token retry attempt {retryCount} starting after {timespan.TotalMilliseconds} milliseconds. {request.Method} {request.RequestUri}");
                        }));

            return services;
        }
    }
}