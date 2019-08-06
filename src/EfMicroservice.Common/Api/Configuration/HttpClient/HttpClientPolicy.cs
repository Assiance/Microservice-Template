using System.Collections.Generic;

namespace EfMicroservice.Common.Api.Configuration.HttpClient
{
    public class HttpClientPolicy
    {
        public string Name { get; set; }

        public RetryConfiguration Retry { get; set; }

        public int? RequestTimeoutMs { get; set; }

        public CircuitBreakerConfiguration CircuitBreaker { get; set; }

        public BulkheadConfiguration Bulkhead { get; set; }

        public IEnumerable<ClientConfiguration> Clients { get; set; }
    }
}