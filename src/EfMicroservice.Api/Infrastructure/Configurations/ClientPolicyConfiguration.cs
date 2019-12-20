using Microsoft.Extensions.DependencyInjection;
using Omni.BuildingBlocks.Api.Configuration.HttpClient;
using Omni.BuildingBlocks.Http.Handlers;
using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace EfMicroservice.Api.Infrastructure.Configurations
{
    public static class ClientPolicyConfiguration
    {
        public static IHttpClientBuilder AddPolicy(this IHttpClientBuilder builder, HttpClientPolicy policy)
        {
            var policiesToWrap = new List<IAsyncPolicy<HttpResponseMessage>>();

            var timeout = ConfigureTimeoutPolicy(policy.RequestTimeoutMs);

            var readRetry = ConfigureRetryReadPolicy(policy.Retry?.Read);
            var writeRetry = ConfigureRetryWritePolicy(policy.Retry?.Write);

            var circuitBreaker = ConfigureCircuitBreakerPolicy(policy.CircuitBreaker);
            policiesToWrap.Add(circuitBreaker);

            var bulkhead = ConfigureBulkheadPolicy(policy.Bulkhead);
            policiesToWrap.Add(bulkhead);

            return builder.AddPolicyHandler(request =>
            {
                var method = request.Method;
                if (method == HttpMethod.Get)
                {
                    return timeout.WrapAsync(
                        readRetry.WrapAsync(Policy.WrapAsync(policiesToWrap.ToArray())));
                }

                if (writeRetry != null && (method == HttpMethod.Put || method == HttpMethod.Delete))
                {
                    return timeout.WrapAsync(
                        writeRetry.WrapAsync(Policy.WrapAsync(policiesToWrap.ToArray())));
                }

                return timeout.WrapAsync(Policy.WrapAsync(policiesToWrap.ToArray()));
            })
                .AddHttpMessageHandler<AppendCorrelationIdHeaderHandler>();
        }

        public static AsyncBulkheadPolicy<HttpResponseMessage> ConfigureBulkheadPolicy(BulkheadConfiguration bulkheadConfig)
        {
            AsyncBulkheadPolicy<HttpResponseMessage> bulkhead = null;
            if (bulkheadConfig != null)
            {
                bulkhead = Policy.BulkheadAsync<HttpResponseMessage>(bulkheadConfig.MaxParallelization,
                    bulkheadConfig.MaxQueuingActions,
                    async (context) =>
                    {
                        Log.Logger.Warning(
                            $"{context.PolicyKey}: Bulkhead rejected. The client capacity has been exceeded.");
                    });
            }

            return bulkhead;
        }

        public static AsyncCircuitBreakerPolicy<HttpResponseMessage> ConfigureCircuitBreakerPolicy(CircuitBreakerConfiguration circuitBreakerConfig)
        {
            AsyncCircuitBreakerPolicy<HttpResponseMessage> circuitBreaker = null;
            if (circuitBreakerConfig != null)
            {
                circuitBreaker = HttpPolicyExtensions.HandleTransientHttpError()
                    .CircuitBreakerAsync(circuitBreakerConfig.ExceptionsAllowedBeforeBreaking,
                        TimeSpan.FromMilliseconds(circuitBreakerConfig.DurationOfBreakMs),
                        (result, timespan, context) =>
                        {
                            var request = result.Result.RequestMessage;
                            Log.Logger.Warning(
                                $"{context.PolicyKey}: Breaking the circuit for {timespan.TotalSeconds} seconds. {request.Method} {request.RequestUri}");
                        }, context => { Log.Logger.Warning($"{context.PolicyKey}: Closing the circuit."); });
            }

            return circuitBreaker;
        }

        public static AsyncTimeoutPolicy<HttpResponseMessage> ConfigureTimeoutPolicy(int? requestTimeoutMs)
        {
            var timeout = Policy.TimeoutAsync<HttpResponseMessage>(
                requestTimeoutMs.HasValue
                    ? TimeSpan.FromMilliseconds(requestTimeoutMs.Value)
                    : TimeSpan.FromMinutes(1),
                async (context, timespan, task) =>
                {
                    Log.Logger.Warning(
                        $"{context.PolicyKey}: execution timed out after {timespan.TotalSeconds} seconds.");
                });

            return timeout;
        }

        public static AsyncRetryPolicy<HttpResponseMessage> ConfigureRetryWritePolicy(RetryRequestConfiguration writeConfig)
        {
            AsyncRetryPolicy<HttpResponseMessage> writeRetry = null;
            if (writeConfig != null)
            {
                var writeTimes = writeConfig.IntervalsMs?.Select(ms => TimeSpan.FromMilliseconds(ms));

                writeRetry = Policy.HandleResult<HttpResponseMessage>(response =>
                        writeConfig.HttpStatusCodes.Any(code =>
                            Enum.Parse<HttpStatusCode>(code) == response.StatusCode))
                    .WaitAndRetryAsync(writeTimes ?? new List<TimeSpan>(),
                        (result, timespan, retryCount, context) =>
                        {
                            var request = result.Result.RequestMessage;
                            Log.Logger.Warning(
                                $"{context.PolicyKey}: (write) retry attempt {retryCount} starting after {timespan.TotalMilliseconds} milliseconds. {request.Method} {request.RequestUri}");
                        });
            }

            return writeRetry;
        }

        public static AsyncRetryPolicy<HttpResponseMessage> ConfigureRetryReadPolicy(RetryRequestConfiguration readConfig)
        {
            var intervals = readConfig?.IntervalsMs ?? new List<int>() { 100, 500 };
            var readTimes = intervals.Select(ms => TimeSpan.FromMilliseconds(ms));

            var readRetry = Policy.HandleResult<HttpResponseMessage>(response =>
                    readConfig.HttpStatusCodes.Any(code =>
                        Enum.Parse<HttpStatusCode>(code) == response.StatusCode))
                .WaitAndRetryAsync(readTimes,
                    ((result, timespan, retryCount, context) =>
                    {
                        var request = result.Result.RequestMessage;
                        Log.Logger.Warning(
                            $"{context.PolicyKey}: (read) retry attempt {retryCount} starting after {timespan.TotalMilliseconds} milliseconds. {request.Method} {request.RequestUri}");
                    }));

            return readRetry;
        }
    }
}