using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using EfMicroservice.Common.Api.Configuration.HttpClient;
using Microsoft.Extensions.Options;

namespace EfMicroservice.Common.Http
{
    public abstract class BaseHttpClient
    {
        protected readonly HttpClient _httpClient;

        public BaseHttpClient(Type childType, HttpClient httpClient, IOptions<List<HttpClientPolicy>> clientPolicies)
        {
            var client = clientPolicies.Value.GetClient(childType);
            httpClient.BaseAddress = new Uri(client.Url);

            _httpClient = httpClient;
        }
    }
}
