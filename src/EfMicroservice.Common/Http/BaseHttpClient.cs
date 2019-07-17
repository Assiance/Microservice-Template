using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EfMicroservice.Common.Api.Configuration.HttpClient;
using EfMicroservice.Common.Http.Client.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

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

        private void ValidateUrlString(string url, UriKind uriKind)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));
            if (!Uri.IsWellFormedUriString(url, uriKind))
                throw new ArgumentException("URI is not well formed.", nameof(url));
        }


        private T DeserializeObject<T>(string jsonObject,string methodName)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(jsonObject);
            }
            catch (JsonSerializationException e)
            {
                _logger.LogWarning($"HttpClient {methodName} request response deserialization error {e.Message}");
                return default(T);
            }
        }

        private async Task<string> GetErrorMessage(HttpResponseMessage response)
        {
            var errorResult = await response.Content.ReadAsStringAsync();
            var type = response.Content.Headers.ContentType;
            string responseErrorMessage;
            if (type.MediaType == MediaType)
            {
                IDictionary<string, JToken> data = (JObject)JsonConvert.DeserializeObject(errorResult);
                var jsonErrorMessageExist = data.TryGetValue("message", out JToken jsonErrorResult);
                responseErrorMessage = jsonErrorMessageExist
                    ? jsonErrorResult.ToString(Formatting.None)
                    : response.ReasonPhrase;

            }
            else
            {
                responseErrorMessage = errorResult;
            }

            return responseErrorMessage;
        }

        public async Task<TResult> PutAsync<T, TResult>(T item, string url)
        {
            ValidateUrlString(url, UriKind.Relative);
            var uri = new Uri(url, UriKind.Relative);
            var json = JsonConvert.SerializeObject(item,
                new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()});
            var content = new StringContent(json, Encoding.UTF8, MediaType);
            var response = await _httpClient.PutAsync(uri, content);

            if (response.IsSuccessStatusCode)
            {
                return DeserializeObject<TResult>(response.Content.ReadAsStringAsync().Result, nameof(this.PutAsync));
            }

            var responseErrorMessage = await GetErrorMessage(response);
            _logger.LogInformation($"HttpClient Put request MessageError: {responseErrorMessage}");
            throw new HttpRequestException(responseErrorMessage);
        }

        public async Task<TResult> PostAsync<T, TResult>(T item, string url)
        {
            ValidateUrlString(url, UriKind.Relative);
            var uri = new Uri(url, UriKind.Relative);
            var json = JsonConvert.SerializeObject(item,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var content = new StringContent(json, Encoding.UTF8, MediaType);
            var response = await _httpClient.PostAsync(uri, content);

            if (response.IsSuccessStatusCode)
            {
                return DeserializeObject<TResult>(response.Content.ReadAsStringAsync().Result, nameof(this.PostAsync));
            }

            var responseErrorMessage = await GetErrorMessage(response);

            _logger.LogInformation($"HttpClient Post request MessageError: {responseErrorMessage}");
            throw new HttpRequestException(responseErrorMessage);
        }

        public async Task<TResult> PatchAsync<T, TResult>(T item, string url)
        {
            ValidateUrlString(url, UriKind.Relative);
            var uri = new Uri(url, UriKind.Relative);
            var json = JsonConvert.SerializeObject(item,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            var content = new StringContent(json, Encoding.UTF8, MediaType);
            var response = await _httpClient.PatchAsync(uri, content);

            if (response.IsSuccessStatusCode)
            {
                return DeserializeObject<TResult>(response.Content.ReadAsStringAsync().Result, nameof(this.PutAsync));
            }

            var responseErrorMessage = await GetErrorMessage(response);
            _logger.LogInformation($"HttpClient Patch request MessageError: {responseErrorMessage}");
            throw new HttpRequestException(responseErrorMessage);
        }

        public async Task<TResult> GetAsync<TResult>(string url)
        {
            ValidateUrlString(url, UriKind.Relative);
            var uri = new Uri(url, UriKind.Relative);
            var response = await _httpClient.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                return DeserializeObject<TResult>(response.Content.ReadAsStringAsync().Result, nameof(this.GetAsync));
            }

            throw new HttpRequestException("something happen making get request");
        }
    }
}

