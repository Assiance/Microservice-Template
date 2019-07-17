using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using EfMicroservice.Common.Api.Configuration.HttpClient;
using EfMicroservice.Common.Http;
using EfMicroservice.Domain.Products;
using EfMicroservice.ExternalData.Clients.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EfMicroservice.ExternalData.Clients
{
    public class GitHaubClient : BaseHttpClient, IGitHaubClient
    {
        private readonly ILogger _logger;

        public GitHaubClient(HttpClient httpClient, IOptions<List<HttpClientPolicy>> clientPolicies, ILoggerFactory loggerFactory)
            : base(typeof(GitHaubClient), httpClient, clientPolicies, loggerFactory)
        {
        
        }

        public async Task<object> Get()
        {          
            var response= await GetAsync<Product>("api/v1/products/aa22c300-7870-4488-ae79-597f8422d964");
            var result = response;
            return result;
        }
    }
}
