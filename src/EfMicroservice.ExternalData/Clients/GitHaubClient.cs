﻿using EfMicroservice.Domain.Products;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Omni.BuildingBlocks.Http.Client;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EfMicroservice.Application.Products.Clients;

namespace EfMicroservice.ExternalData.Clients
{
    public class GitHaubClient : BaseHttpClient, IGitHaubClient
    {
        public GitHaubClient(HttpClient httpClient, ILoggerFactory loggerFactory)
            : base(httpClient, loggerFactory.CreateLogger<GitHaubClient>())
        {
        }

        public async Task<object> Get()
        {
            var response = await GetAsync<Product>("api/v1/products/5ab8aa9b-9336-4696-98c1-471b57112de3");
            var result = response;
            return result;
        }

        public async Task<object> SendAsyncDoesGet()
        {
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("api/v1/products/5ab8aa9b-9336-4696-98c1-471b57112de3", UriKind.Relative),
            };
            var response = await SendAsync<Product>(request);
            var result = response;
            return result;
        }

        public async Task<object> SendAsyncDoesPost()
        {
            var json = JsonConvert.SerializeObject(new { name = "testmarlo132n", price = 12324, quantity = 1 },
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

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