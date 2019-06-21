using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using EfMicroservice.Core.Api.Configuration.HttpClient;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using Serilog;

namespace EfMicroservice.Api.Configurations
{
    public static class ClientPolicyConfiguration
    {
        public static void RegisterClients(this IServiceCollection services, List<HttpClientPolicy> policies, Dictionary<Type, Func<IServiceCollection, IHttpClientBuilder>> clientDict)
        {
            var dataAssembly = Assembly.Load("EfMicroservice.Data");

            foreach (var policy in policies)
            {
                var policiesToWrap = new List<IAsyncPolicy<HttpResponseMessage>>();

                var timeout = ConfigureTimeoutPolicy(policy);

                var readRetry = ConfigureRetryReadPolicy(policy);
                var writeRetry = ConfigureRetryWritePolicy(policy);

                var circuitBreaker = ConfigureCircuitBreakerPolicy(policy);
                policiesToWrap.Add(circuitBreaker);

                var bulkhead = ConfigureBulkheadPolicy(policy);
                policiesToWrap.Add(bulkhead);

                foreach (var client in policy.Clients)
                {
                    var clientType = dataAssembly.GetType(client.Namespace);
                    var clientBuilder = clientDict[clientType](services);

                    clientBuilder.AddPolicyHandler(request =>
                    {
                        var method = request.Method;
                        if (method == HttpMethod.Get)
                        {
                            return timeout.WrapAsync(readRetry.WrapAsync(Policy.WrapAsync(policiesToWrap.ToArray())));
                        }

                        if (writeRetry != null && (method == HttpMethod.Put || method == HttpMethod.Delete))
                        {
                            return timeout.WrapAsync(writeRetry.WrapAsync(Policy.WrapAsync(policiesToWrap.ToArray())));
                        }

                        return timeout.WrapAsync(Policy.WrapAsync(policiesToWrap.ToArray()));
                    });
                }
            }
        }
        private static AsyncBulkheadPolicy<HttpResponseMessage> ConfigureBulkheadPolicy(HttpClientPolicy policy)
        {
            AsyncBulkheadPolicy<HttpResponseMessage> bulkhead = null;
            if (policy.Bulkhead != null)
            {
                bulkhead = Policy.BulkheadAsync<HttpResponseMessage>(policy.Bulkhead.MaxParallelization,
                    policy.Bulkhead.MaxQueuingActions,
                    async (context) =>
                    {
                        Log.Logger.Warning($"{context.PolicyKey}: Bulkhead rejected. The client capacity has been exceeded.");
                    });
            }

            return bulkhead;
        }

        private static AsyncCircuitBreakerPolicy<HttpResponseMessage> ConfigureCircuitBreakerPolicy(HttpClientPolicy policy)
        {
            AsyncCircuitBreakerPolicy<HttpResponseMessage> circuitBreaker = null;
            if (policy.CircuitBreaker != null)
            {
                circuitBreaker = HttpPolicyExtensions.HandleTransientHttpError()
                    .CircuitBreakerAsync(policy.CircuitBreaker.ExceptionsAllowedBeforeBreaking,
                        TimeSpan.FromMilliseconds(policy.CircuitBreaker.DurationOfBreakMs),
                        (result, timespan, context) =>
                        {
                            var request = result.Result.RequestMessage;
                            Log.Logger.Warning($"{context.PolicyKey}: Breaking the circuit for {timespan.TotalSeconds} seconds. {request.Method} {request.RequestUri}");
                        }, context =>
                        {
                            Log.Logger.Warning($"{context.PolicyKey}: Closing the circuit.");
                        });
            }

            return circuitBreaker;
        }

        private static AsyncTimeoutPolicy<HttpResponseMessage> ConfigureTimeoutPolicy(HttpClientPolicy policy)
        {
            var timeout = Policy.TimeoutAsync<HttpResponseMessage>(
                policy.RequestTimeoutMs.HasValue
                    ? TimeSpan.FromMilliseconds(policy.RequestTimeoutMs.Value)
                    : TimeSpan.FromMinutes(1),
                async (context, timespan, task) =>
                {
                    Log.Logger.Warning($"{context.PolicyKey}: execution timed out after {timespan.TotalSeconds} seconds.");
                });

            return timeout;
        }

        private static AsyncRetryPolicy<HttpResponseMessage> ConfigureRetryWritePolicy(HttpClientPolicy policy)
        {
            AsyncRetryPolicy<HttpResponseMessage> writeRetry = null;
            if (policy.Retry?.Write != null)
            {
                var writeTimes = policy.Retry?.Write?.IntervalsMs?.Select(ms => TimeSpan.FromMilliseconds(ms));

                writeRetry = Policy.HandleResult<HttpResponseMessage>(response => policy.Retry.Write.HttpStatusCodes.Any(code => Enum.Parse<HttpStatusCode>(code) == response.StatusCode))
                    .WaitAndRetryAsync(writeTimes ?? new List<TimeSpan>(),
                        (result, timespan, retryCount, context) =>
                        {
                            var request = result.Result.RequestMessage;
                            Log.Logger.Warning($"{context.PolicyKey}: (write) retry attempt {retryCount} starting after {timespan.TotalMilliseconds} milliseconds. {request.Method} {request.RequestUri}");
                        });
            }

            return writeRetry;
        }

        private static AsyncRetryPolicy<HttpResponseMessage> ConfigureRetryReadPolicy(HttpClientPolicy policy)
        {
            var intervals = policy.Retry?.Read?.IntervalsMs ?? new List<int>() { 100, 500 };
            var readTimes = intervals.Select(ms => TimeSpan.FromMilliseconds(ms));

            var readRetry = Policy.HandleResult<HttpResponseMessage>(response => policy.Retry.Read.HttpStatusCodes.Any(code => Enum.Parse<HttpStatusCode>(code) == response.StatusCode))
                .WaitAndRetryAsync(readTimes,
                    ((result, timespan, retryCount, context) =>
                    {
                        var request = result.Result.RequestMessage;
                        Log.Logger.Warning($"{context.PolicyKey}: (read) retry attempt {retryCount} starting after {timespan.TotalMilliseconds} milliseconds. {request.Method} {request.RequestUri}");
                    }));

            return readRetry;
        }
    }
}
