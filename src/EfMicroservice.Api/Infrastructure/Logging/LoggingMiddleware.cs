using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Omni.BuildingBlocks.Http.CorrelationId;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace EfMicroservice.Api.Infrastructure.Logging
{
    public class LoggingMiddleware : IMiddleware
    {
        private readonly ICorrelationIdProvider _correlationIdProvider;

        private static readonly HashSet<string> HeaderWhitelist = new HashSet<string>
            {"Content-Type", "Content-Length", "User-Agent"};

        static readonly ILogger Log = Serilog.Log.ForContext<LoggingMiddleware>();

        public LoggingMiddleware(ICorrelationIdProvider correlationIdProvider)
        {
            _correlationIdProvider = correlationIdProvider;
        }

        public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var start = Stopwatch.GetTimestamp();

            PushInfoToContext(httpContext, _correlationIdProvider);

            var requestStartingLog = GenerateRequestStartingLogMessage(httpContext);
            Log.Information(requestStartingLog);

            try
            {
                await next(httpContext);

                var elapsedMs = GetElapsedMilliseconds(start, Stopwatch.GetTimestamp());

                var statusCode = httpContext.Response?.StatusCode;
                var level = statusCode >= 500 ? LogEventLevel.Error : LogEventLevel.Information;

                var log = level == LogEventLevel.Error ? LogForErrorContext(httpContext) : Log;
                var requestFinishingLog = GenerateRequestFinishingLogMessage(httpContext, elapsedMs);
                log.Write(level, requestFinishingLog);
            }
            catch (Exception ex) when (LogException(httpContext,
                GetElapsedMilliseconds(start, Stopwatch.GetTimestamp()), ex))
            {
            }
        }

        private static void PushInfoToContext(HttpContext httpContext, ICorrelationIdProvider correlationIdProvider)
        {
            LogContext.PushProperty("Info", new
            {
                MachineName = Environment.MachineName,
                ClientIP = httpContext.Connection.RemoteIpAddress,
                RequestId = Guid.NewGuid(),
                CorrelationId = correlationIdProvider.EnsureCorrelationIdPresent()
            });
        }

        private static string GenerateRequestStartingLogMessage(HttpContext httpContext)
        {
            var request = httpContext.Request;
            return string.Format(
                CultureInfo.InvariantCulture,
                "Request starting {0} {1} {2}://{3}{4}{5}{6} {7} {8} {9}",
                request.Protocol,
                request.Method,
                request.Scheme,
                request.Host.Value,
                request.PathBase.Value,
                request.Path.Value,
                request.QueryString.Value,
                request.ContentType,
                request.ContentLength, new
                {
                    Method = httpContext.Request.Method,
                    RequestUrl = httpContext.Request.Path
                });
        }

        private static string GenerateRequestFinishingLogMessage(HttpContext httpContext, double elapsed,
            int? statusCode = null)
        {
            var request = httpContext.Request;
            var response = httpContext.Response;

            return string.Format(
                CultureInfo.InvariantCulture,
                "Request finished in {0:0.0000}ms {1} {2} {3}",
                elapsed,
                statusCode ?? response.StatusCode,
                response.ContentType,
                new
                {
                    Method = request.Method,
                    RequestUrl = request.Path,
                    StatusCode = statusCode ?? response.StatusCode,
                    ResponseTimeMs = elapsed
                });
        }

        private static bool LogException(HttpContext httpContext, double elapsedMs, Exception ex)
        {
            var log = LogForErrorContext(httpContext);
            var requestFinishingLog = GenerateRequestFinishingLogMessage(httpContext, elapsedMs, 500);
            log.Error(ex, requestFinishingLog);

            return false;
        }

        private static ILogger LogForErrorContext(HttpContext httpContext)
        {
            var request = httpContext.Request;

            var loggedHeaders = request.Headers
                .Where(h => HeaderWhitelist.Contains(h.Key))
                .ToDictionary(h => h.Key, h => h.Value.ToString());

            var result = Log
                .ForContext("RequestHeaders", loggedHeaders, destructureObjects: true)
                .ForContext("RequestHost", request.Host)
                .ForContext("RequestProtocol", request.Protocol);

            if (request.HasFormContentType)
            {
                result = result.ForContext("RequestForm",
                    request.Form.ToDictionary(v => v.Key, v => v.Value.ToString()));
            }

            return result;
        }

        private static double GetElapsedMilliseconds(long start, long stop)
        {
            return (stop - start) * 1000 / (double) Stopwatch.Frequency;
        }
    }
}