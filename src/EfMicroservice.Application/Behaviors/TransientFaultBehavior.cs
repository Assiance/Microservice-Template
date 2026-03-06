using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EfMicroservice.Application.Behaviors
{
    public class TransientFaultBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<TransientFaultBehavior<TRequest, TResponse>> _logger;

        private static readonly ResiliencePipeline<TResponse> RetryPipeline =
            new ResiliencePipelineBuilder<TResponse>()
                .AddRetry(new RetryStrategyOptions<TResponse>
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromMilliseconds(100),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                    ShouldHandle = new PredicateBuilder<TResponse>()
                        .Handle<DbUpdateException>(IsTransient)
                        .Handle<TimeoutException>()
                })
                .Build();

        public TransientFaultBehavior(ILogger<TransientFaultBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!request.IsCommand())
                return await next();

            return await RetryPipeline.ExecuteAsync(
                async ct =>
                {
                    return await next();
                },
                cancellationToken);
        }

        private static bool IsTransient(DbUpdateException ex)
        {
            // Postgres transient error codes: connection failure, deadlock, serialization failure
            if (ex.InnerException is Npgsql.NpgsqlException npgEx)
            {
                return npgEx.IsTransient;
            }
            return false;
        }
    }
}
