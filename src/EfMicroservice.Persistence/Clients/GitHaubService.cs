using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using EfMicroservice.Common.Api.Configuration.HttpClient;
using EfMicroservice.Persistence.Clients.Interfaces;
using Microsoft.Extensions.Options;

namespace EfMicroservice.Persistence.Clients
{
    public class GitHaubService : IGitHaubService
    {
        private readonly HttpClient _httpClient;

        public GitHaubService(HttpClient httpClient, IOptions<List<HttpClientPolicy>> clientPolicies)
        {
            var client = clientPolicies.Value.GetClient(typeof(GitHaubService));
            httpClient.BaseAddress = new Uri(client.Url);

            _httpClient = httpClient;
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
