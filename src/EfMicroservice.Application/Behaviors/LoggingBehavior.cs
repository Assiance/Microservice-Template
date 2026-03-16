using MediatR;
using Microsoft.Extensions.Logging;
using Omni.BuildingBlocks.Http.CorrelationId;
using Omni.BuildingBlocks.Identity;
using Omni.BuildingBlocks.Observability;
using OpenTelemetry.Trace;
using Serilog.Context;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace EfMicroservice.Application.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICorrelationIdProvider _correlationIdProvider;

        public LoggingBehavior(
            ILogger<LoggingBehavior<TRequest, TResponse>> logger,
            ICurrentUserService currentUserService,
            ICorrelationIdProvider correlationIdProvider)
        {
            _logger = logger;
            _currentUserService = currentUserService;
            _correlationIdProvider = correlationIdProvider;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var correlationId = _correlationIdProvider.EnsureCorrelationIdPresent();
            var userId = _currentUserService.GetCurrentUser()?.Email ?? "anonymous";

            using var activity = ActivitySources.Default.StartActivity($"MediatR/{requestName}");
            activity?.SetTag("mediator.request.type", typeof(TRequest).FullName);
            activity?.SetTag("mediator.request.name", requestName);
            activity?.SetTag("app.user.id", userId);
            activity?.SetTag("app.correlation_id", correlationId);

            using (LogContext.PushProperty("RequestType", requestName))
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    var response = await next();
                    sw.Stop();

                    activity?.SetStatus(ActivityStatusCode.Ok);
                    _logger.LogInformation(
                        "MediatR request {RequestName} completed in {ElapsedMs}ms [CorrelationId: {CorrelationId}]",
                        requestName, sw.ElapsedMilliseconds, correlationId);

                    return response;
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    activity?.RecordException(ex);

                    _logger.LogError(ex,
                        "MediatR request {RequestName} failed after {ElapsedMs}ms [CorrelationId: {CorrelationId}]",
                        requestName, sw.ElapsedMilliseconds, correlationId);

                    throw;
                }
            }
        }
    }
}
