using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using EfMicroservice.Common.Api.Configuration.HttpClient;
using EfMicroservice.Common.Http;
using EfMicroservice.ExternalData.Clients.Interfaces;
using Microsoft.Extensions.Options;

namespace EfMicroservice.ExternalData.Clients
{
    public class GitHaubClient : BaseHttpClient, IGitHaubClient
    {
        public GitHaubClient(HttpClient httpClient, IOptions<List<HttpClientPolicy>> clientPolicies)
            : base(typeof(GitHaubClient), httpClient, clientPolicies)
        {
        }

        public async Task<object> Get()
        {
            var response = await _httpClient.GetAsync("api/v1/products/aa22c300-7870-4488-ae79-597f8422d964");

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsAsync<object>();

            return result;
        }
    }
}
