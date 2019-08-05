using EfMicroservice.Common.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EfMicroservice.Common.Authentication
{
    public class AccessTokenProvider : BaseHttpClient, IAccessTokenProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccessTokenProvider(IHttpContextAccessor httpContextAccessor, HttpClient httpClient, ILoggerFactory loggerFactory) : base(httpClient, loggerFactory.CreateLogger<AccessTokenProvider>())
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> RefreshAccessTokenAsync(string tokenEndpointUrl, string clientId, string clientSecret)
        {
            var refreshToken = _httpContextAccessor.HttpContext.Request.Headers[KnownHttpHeaders.RefreshToken].FirstOrDefault();
            return await RefreshAccessTokenAsync(tokenEndpointUrl, clientId, clientSecret, refreshToken);
        }

        public async Task<string> RefreshAccessTokenAsync(string tokenEndpointUrl, string clientId, string clientSecret, string refreshToken)
        {
            var body = new Dictionary<string, string>();
            body.Add("grant_type", "refresh_token");
            body.Add("client_id", clientId);
            body.Add("client_secret", clientSecret);
            body.Add("refresh_token", refreshToken);

            var tokenRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(tokenEndpointUrl),
                Content = new FormUrlEncodedContent(body)
            };

            var tokenResponse = await SendAsync<TokenResponse>(tokenRequest);
            return tokenResponse?.access_token;
        }
    }
}
