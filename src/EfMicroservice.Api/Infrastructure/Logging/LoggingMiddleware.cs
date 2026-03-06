using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            var req = httpContext.Request;
            Log.Information("Request starting {Method} {Scheme}://{Host}{Path}{QueryString}",
                req.Method, req.Scheme, req.Host.Value, req.Path.Value, req.QueryString.Value);

            try
            {
                await next(httpContext);

                var elapsedMs = GetElapsedMilliseconds(start, Stopwatch.GetTimestamp());
                var statusCode = httpContext.Response?.StatusCode;
                var level = statusCode >= 500 ? LogEventLevel.Error : LogEventLevel.Information;

                var log = level == LogEventLevel.Error ? LogForErrorContext(httpContext) : Log;
                log.Write(level,
                    "Request finished in {ElapsedMs:0.0000}ms {StatusCode} {ContentType}",
                    elapsedMs, statusCode, httpContext.Response.ContentType);
            }
            catch (Exception ex) when (LogException(httpContext,
                GetElapsedMilliseconds(start, Stopwatch.GetTimestamp()), ex))
            {
            }
        }

        private static void PushInfoToContext(HttpContext httpContext, ICorrelationIdProvider correlationIdProvider)
        {
            var traceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;
            var correlationId = correlationIdProvider.EnsureCorrelationIdPresent();

            LogContext.PushProperty("TraceId", traceId);
            LogContext.PushProperty("CorrelationId", correlationId);
            LogContext.PushProperty("MachineName", Environment.MachineName);
            LogContext.PushProperty("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
        }

        private static bool LogException(HttpContext httpContext, double elapsedMs, Exception ex)
        {
            var log = LogForErrorContext(httpContext);
            log.Error(ex, "Request finished in {ElapsedMs:0.0000}ms {StatusCode}", elapsedMs, 500);
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
            return (stop - start) * 1000 / (double)Stopwatch.Frequency;
        }
    }
}
