using MediatR;
using Microsoft.Extensions.Logging;
using Omni.BuildingBlocks.Application.Idempotency;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace EfMicroservice.Application.Behaviors
{
    public class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IIdempotencyStore _idempotencyStore;
        private readonly ILogger<IdempotencyBehavior<TRequest, TResponse>> _logger;

        public IdempotencyBehavior(
            IIdempotencyStore idempotencyStore,
            ILogger<IdempotencyBehavior<TRequest, TResponse>> logger)
        {
            _idempotencyStore = idempotencyStore;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (request is not IIdempotentCommand idempotentCommand)
                return await next();

            var key = idempotentCommand.IdempotencyKey;

            if (await _idempotencyStore.ExistsAsync(key, cancellationToken))
            {
                _logger.LogInformation(
                    "Idempotent request {RequestType} with key {IdempotencyKey} already processed — returning cached response",
                    typeof(TRequest).Name, key);

                var cachedJson = await _idempotencyStore.GetResponseAsync(key, cancellationToken);
                return cachedJson != null
                    ? JsonSerializer.Deserialize<TResponse>(cachedJson)
                    : default;
            }

            using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            var response = await next();

            var responseJson = JsonSerializer.Serialize(response);
            await _idempotencyStore.SetAsync(key, typeof(TRequest).Name, responseJson, cancellationToken);

            transactionScope.Complete();
            return response;
        }
    }
}
