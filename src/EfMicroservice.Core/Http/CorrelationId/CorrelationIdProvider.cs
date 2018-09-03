using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace EfMicroservice.Core.Http.CorrelationId
{
    public class CorrelationIdProvider : ICorrelationIdProvider
    {
        private readonly ILogger _logger;

        public CorrelationIdProvider(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(CorrelationIdProvider));
        }

        public string EnsureCorrelationIdPresent(HttpRequest request)
        {
            request.Headers.TryGetValue(KnownHttpHeaders.CorrelationId, out StringValues values);
            var correlationId = values.FirstOrDefault();

            if (string.IsNullOrEmpty(correlationId))
            {
                correlationId = Guid.NewGuid().ToString();
                _logger.LogInformation($"{KnownHttpHeaders.CorrelationId} header is not set. New CorrelationId is generated {correlationId}");
            }

            return correlationId;
        }
    }
}
