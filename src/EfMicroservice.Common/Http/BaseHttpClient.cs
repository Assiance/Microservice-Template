using EfMicroservice.Common.Api.Configuration.HttpClient;
using EfMicroservice.Common.ExceptionHandling.Exceptions;
using EfMicroservice.Common.Http.Client.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EfMicroservice.Common.Http
{
    public abstract class BaseHttpClient : IBaseHttpClient
    {
        protected readonly HttpClient _httpClient;
        private const string MediaType = "application/json";
        private readonly ILogger _logger;

        public BaseHttpClient(Type childType, HttpClient httpClient, IOptions<List<HttpClientPolicy>> clientPolicies,
            ILoggerFactory loggerFactory)
        {
            var client = clientPolicies.Value.GetClient(childType);
            httpClient.BaseAddress = new Uri(client.Url);

            _httpClient = httpClient;
            _logger = loggerFactory.CreateLogger<BaseHttpClient>();
        }

        public async Task<TResult> PutAsync<T, TResult>(T item, string url)
        {
            var uri = TryGetUri(url);
            var content = GetStringContent(item);
            var response = await _httpClient.PutAsync(uri, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return TryDeserializeObject<TResult>(responseContent, nameof(this.PutAsync));
            }

            throw new HttpCallException(uri, response.RequestMessage.Method, response.StatusCode, response.ReasonPhrase,
                responseContent);
        }

        public async Task<TResult> SendAsync<TResult>(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(HttpResponseMessage));
            }

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return TryDeserializeObject<TResult>(responseContent, nameof(this.SendAsync));
            }

            throw new HttpCallException(request.RequestUri, response.RequestMessage.Method, response.StatusCode,
                response.ReasonPhrase, responseContent);
        }

        public async Task<TResult> PostAsync<T, TResult>(T item, string url)
        {
            var uri = TryGetUri(url);
            var content = GetStringContent(item);
            var response = await _httpClient.PostAsync(uri, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return TryDeserializeObject<TResult>(responseContent, nameof(this.PostAsync));
            }

            throw new HttpCallException(uri, response.RequestMessage.Method, response.StatusCode, response.ReasonPhrase,
                responseContent);
        }

        public async Task<TResult> PatchAsync<T, TResult>(T item, string url)
        {
            var uri = TryGetUri(url);
            var content = GetStringContent(item);
            var response = await _httpClient.PatchAsync(uri, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return TryDeserializeObject<TResult>(responseContent, nameof(this.PutAsync));
            }

            throw new HttpCallException(uri, response.RequestMessage.Method, response.StatusCode, response.ReasonPhrase,
                responseContent);
        }

        public async Task<TResult> GetAsync<TResult>(string url)
        {
            var uri = TryGetUri(url);
            var response = await _httpClient.GetAsync(uri);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return TryDeserializeObject<TResult>(response.Content.ReadAsStringAsync().Result, nameof(this.GetAsync));
            }

            throw new HttpCallException(uri, response.RequestMessage.Method, response.StatusCode, response.ReasonPhrase,
                responseContent);
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
                throw new ArgumentException($"HttpClient {methodName} request response deserialization error {e.Message}");
            }
        }
    }
}

