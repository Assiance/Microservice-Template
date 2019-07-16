using System.Threading.Tasks;
using EfMicroservice.Common.Http;
using EfMicroservice.Common.Http.CorrelationId;
using Microsoft.AspNetCore.Http;

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
            var correlation = correlationIdProvider.EnsureCorrelationIdPresent(context.Request);
            var request = context.Request;
            var response = context.Response;
          
            request.Headers.Add(KnownHttpHeaders.CorrelationId,correlation);
            response.Headers.Add(KnownHttpHeaders.CorrelationId, correlation);
            await _next(context);
        }
    }
}
