using Microsoft.Extensions.Logging;
using Omni.BuildingBlocks.ExceptionHandling.Exceptions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EfMicroservice.Api.Infrastructure.Handlers
{
    public class UnsuccessfulResponseHandler : DelegatingHandler
    {
        private readonly ILogger<UnsuccessfulResponseHandler> _logger;

        public UnsuccessfulResponseHandler(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UnsuccessfulResponseHandler>();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            throw new HttpCallException(request.RequestUri, response.RequestMessage.Method, response.StatusCode,
                response.ReasonPhrase, responseContent);
        }
    }
}