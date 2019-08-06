using EfMicroservice.Common.Http;
using EfMicroservice.Common.Http.CorrelationId;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Linq;
using System.Threading.Tasks;

namespace EfMicroservice.Api.Infrastructure.Logging
{
    public class AddCorrelationIdToHeaderMiddleware
    {
        private readonly RequestDelegate _next;

        public AddCorrelationIdToHeaderMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ICorrelationIdProvider correlationIdProvider)
        {
            var correlation = correlationIdProvider.EnsureCorrelationIdPresent();
            var request = context.Request;
            var response = context.Response;

            request.Headers.TryGetValue(KnownHttpHeaders.CorrelationId, out StringValues requestHeaderValue);
            if (requestHeaderValue.FirstOrDefault() == null)
            {
                request.Headers.Add(KnownHttpHeaders.CorrelationId, correlation);
            }

            response.Headers.TryGetValue(KnownHttpHeaders.CorrelationId, out StringValues responseHeaderValue);
            if (responseHeaderValue.FirstOrDefault() == null)
            {
                response.Headers.Add(KnownHttpHeaders.CorrelationId, correlation);
            }

            await _next(context);
        }
    }
}
