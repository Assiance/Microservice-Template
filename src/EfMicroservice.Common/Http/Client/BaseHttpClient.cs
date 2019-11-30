using EfMicroservice.Common.Api.Configuration.HttpClient;
using EfMicroservice.Common.Http.Client.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EfMicroservice.Common.Http.Client
{
    public abstract class BaseHttpClient : IBaseHttpClient
    {
        protected readonly HttpClient _httpClient;
        protected readonly ILogger _logger;
        private const string MediaType = "application/json";

        public BaseHttpClient(Type childType, HttpClient httpClient, IOptions<List<HttpClientPolicy>> clientPolicies,
            ILogger logger)
        {
            var client = clientPolicies.Value.GetClient(childType);
            httpClient.BaseAddress = new Uri(client.BaseUrl);
            httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue(MediaType));

            _httpClient = httpClient;
            _logger = logger;
        }

        public BaseHttpClient(HttpClient httpClient, ILogger logger)
        {
            httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue(MediaType));

            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<TResult> PutAsync<T, TResult>(T item, string url)
        {
            var uri = TryGetUri(url);
            var content = GetStringContent(item);
            var response = await _httpClient.PutAsync(uri, content);

            return TryDeserializeObject<TResult>(response.Content.ReadAsStringAsync().Result, nameof(this.PutAsync));
        }

        public async Task<TResult> SendAsync<TResult>(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(HttpResponseMessage));
            }

            var response = await _httpClient.SendAsync(request);

            return TryDeserializeObject<TResult>(response.Content.ReadAsStringAsync().Result, nameof(this.SendAsync));
        }

        public async Task<TResult> PostAsync<T, TResult>(T item, string url)
        {
            var uri = TryGetUri(url);
            var content = GetStringContent(item);
            var response = await _httpClient.PostAsync(uri, content);

            return TryDeserializeObject<TResult>(response.Content.ReadAsStringAsync().Result, nameof(this.PostAsync));
        }

        public async Task<TResult> PatchAsync<T, TResult>(T item, string url)
        {
            var uri = TryGetUri(url);
            var content = GetStringContent(item);
            var response = await _httpClient.PatchAsync(uri, content);

            return TryDeserializeObject<TResult>(response.Content.ReadAsStringAsync().Result, nameof(this.PutAsync));
        }

        public async Task<TResult> GetAsync<TResult>(string url)
        {
            var uri = TryGetUri(url);
            var response = await _httpClient.GetAsync(uri);

            return TryDeserializeObject<TResult>(response.Content.ReadAsStringAsync().Result, nameof(this.GetAsync));
        }

        private StringContent GetStringContent<T>(T item)
        {
            var json = JsonConvert.SerializeObject(item,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var content = new StringContent(json, Encoding.UTF8, MediaType);
            return content;
        }

        private Uri TryGetUri(string url)
        {
            ValidateUrlString(url, UriKind.Relative);
            var uri = new Uri(url, UriKind.Relative);
            return uri;
        }

        private void ValidateUrlString(string url, UriKind uriKind)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));
            if (!Uri.IsWellFormedUriString(url, uriKind))
                throw new ArgumentException("URI is not well formed.", nameof(url));
        }

        private T TryDeserializeObject<T>(string jsonObject, string methodName)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(jsonObject);
            }
            catch (JsonSerializationException e)
            {
                throw new ArgumentException(
                    $"HttpClient {methodName} request response deserialization error {e.Message}");
            }
        }
    }
}