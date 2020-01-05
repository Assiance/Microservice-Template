using Microsoft.Extensions.DependencyInjection;
using Omni.BuildingBlocks.Api.Configuration.HttpClient;
using Omni.BuildingBlocks.Api.Configuration.HttpClient.Models;
using Omni.BuildingBlocks.Http.Handlers;
using Polly;
using System.Collections.Generic;
using System.Net.Http;

namespace EfMicroservice.Api.Infrastructure.Extensions
{
    public static class ClientPolicyExtensions
    {
        public static IHttpClientBuilder AddPolicy(this IHttpClientBuilder builder, HttpClientPolicy policy)
        {
            var policiesToWrap = new List<IAsyncPolicy<HttpResponseMessage>>();

            var timeout = ClientPolicyConfiguration.ConfigureTimeoutPolicy(policy.RequestTimeoutMs);

            var readRetry = ClientPolicyConfiguration.ConfigureRetryReadPolicy(policy.Retry?.Read);
            var writeRetry = ClientPolicyConfiguration.ConfigureRetryWritePolicy(policy.Retry?.Write);

            var exceptionFallback = ClientPolicyConfiguration.ConfigureExceptionFallbackPolicy();

            var circuitBreaker = ClientPolicyConfiguration.ConfigureCircuitBreakerPolicy(policy.CircuitBreaker);
            policiesToWrap.Add(circuitBreaker);

            var bulkhead = ClientPolicyConfiguration.ConfigureBulkheadPolicy(policy.Bulkhead);
            policiesToWrap.Add(bulkhead);

            return builder.AddPolicyHandler(request =>
                {
                    var method = request.Method;
                    if (method == HttpMethod.Get)
                    {
                        return timeout.WrapAsync(exceptionFallback.WrapAsync(
                            readRetry).WrapAsync(Policy.WrapAsync(policiesToWrap.ToArray())));
                    }

                    if (writeRetry != null && (method == HttpMethod.Put || method == HttpMethod.Delete))
                    {
                        return timeout.WrapAsync(exceptionFallback.WrapAsync(
                            writeRetry).WrapAsync(Policy.WrapAsync(policiesToWrap.ToArray())));
                    }

                    return timeout.WrapAsync(exceptionFallback.WrapAsync(Policy.WrapAsync(policiesToWrap.ToArray())));
                })
                .AddHttpMessageHandler<AppendCorrelationIdHeaderHandler>();
        }
    }
}
