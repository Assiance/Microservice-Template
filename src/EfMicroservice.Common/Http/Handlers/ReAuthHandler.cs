using EfMicroservice.Common.Api.Configuration.HttpClient;
using EfMicroservice.Common.Authentication;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace EfMicroservice.Common.Http.Handlers
{
    public class ReAuthHandler : DelegatingHandler
    {
        private readonly IAccessTokenProvider _accessTokenProvider;
        private readonly IOptions<List<HttpClientPolicy>> _clientPolicies;

        public ReAuthHandler(IAccessTokenProvider accessTokenProvider, IOptions<List<HttpClientPolicy>> clientPolicies)
        {
            _accessTokenProvider = accessTokenProvider;
            _clientPolicies = clientPolicies;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var requestUri = request.RequestUri;
                var client = _clientPolicies.Value.GetClient($"{requestUri.Scheme}://{requestUri.Authority}");

                var accessToken = await _accessTokenProvider.RefreshAccessTokenAsync(client.TokenEndpointUrl,
                    client.ClientId, client.ClientSecret);

                if (accessToken != null)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    response = await base.SendAsync(request, cancellationToken);
                }
            }

            return response;
        }
    }
}