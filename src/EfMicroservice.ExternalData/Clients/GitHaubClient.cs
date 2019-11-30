using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EfMicroservice.Common.Api.Configuration.HttpClient;
using EfMicroservice.Common.Http;
using EfMicroservice.Common.Http.Client;
using EfMicroservice.Domain.Products;
using EfMicroservice.ExternalData.Clients.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace EfMicroservice.ExternalData.Clients
{
    public class GitHaubClient : BaseHttpClient, IGitHaubClient
    {
        public GitHaubClient(HttpClient httpClient, IOptions<List<HttpClientPolicy>> clientPolicies,
            ILoggerFactory loggerFactory)
            : base(typeof(GitHaubClient), httpClient, clientPolicies, loggerFactory.CreateLogger<GitHaubClient>())
        {
        }

        public async Task<object> Get()
        {
            var response = await GetAsync<Product>("api/v1/products/41d32cf8-8cb9-446d-b2ed-8585a9ebb856");
            var result = response;
            return result;
        }

        public async Task<object> SendAsyncDoesGet()
        {
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("api/v1/products/41d32cf8-8cb9-446d-b2ed-8585a9ebb856", UriKind.Relative),
            };
            var response = await SendAsync<Product>(request);
            var result = response;
            return result;
        }

        public async Task<object> SendAsyncDoesPost()
        {
            var json = JsonConvert.SerializeObject(new {name = "testmarlon", price = 12324, quantity = 1},
                new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()});

            var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("api/v1/products", UriKind.Relative),
                Content = stringContent
            };
            var response = await SendAsync<Product>(request);
            var result = response;
            return result;
        }
    }
}