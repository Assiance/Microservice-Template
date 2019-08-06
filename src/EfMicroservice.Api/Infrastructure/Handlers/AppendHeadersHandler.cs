using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using EfMicroservice.Common.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace EfMicroservice.Api.Infrastructure.Handlers
{
    public class AppendHeadersHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AppendHeadersHandler> _logger;

        public AppendHeadersHandler(ILoggerFactory loggerFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = loggerFactory.CreateLogger<AppendHeadersHandler>();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            AddCorrelationIdToRequestHeader(request);
            await AddAuthorizationRequestHeaderAsync(request);

            var response = await base.SendAsync(request, cancellationToken);

            return response;
        }

        private void AddCorrelationIdToRequestHeader(HttpRequestMessage request)
        {
            _httpContextAccessor?.HttpContext?.Request?.Headers.TryGetValue(KnownHttpHeaders.CorrelationId,
                out StringValues values);
            var correlationId = values.FirstOrDefault();

            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogWarning($"{KnownHttpHeaders.CorrelationId} header is not set.");
            }

            request.Headers.Add(KnownHttpHeaders.CorrelationId, correlationId);
        }

        private async Task AddAuthorizationRequestHeaderAsync(HttpRequestMessage request)
        {
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning($"{KnownHttpHeaders.Authorization} header is not set.");
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }
    }
}