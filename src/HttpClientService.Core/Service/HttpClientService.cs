using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace HttpClientService.Core.Service
{
    public class HttpClientService : IHttpClientService
    {
        private readonly IHttpClientServiceConfig _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Dictionary<string, string> _defaultRequestHeaders = new Dictionary<string, string>();

        public HttpClientService(IHttpClientServiceConfig config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string route, TRequest dto, CancellationToken token)
        {
            var httpContent = new StringContent(JsonConvert.SerializeObject(dto), new UTF8Encoding(), "application/json");
            var httpResponse = await GetClient().PostAsync(route, httpContent, token);

            if (!httpResponse.IsSuccessStatusCode)
            {

                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }

                var message = await httpResponse.Content.ReadAsStringAsync();
                var error = JsonConvert.DeserializeObject<HttpError>(message);

                throw new HttpClientServiceException(httpResponse.StatusCode, error.Message);
            }
            
            var jsonBody = await httpResponse.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TResponse>(jsonBody);
        }

        public async Task PostAsync<TRequest>(string route, TRequest dto, CancellationToken token)
        {
            var httpContent = new StringContent(JsonConvert.SerializeObject(dto), new UTF8Encoding(), "application/json");
            var httpResponse = await GetClient().PostAsync(route, httpContent, token);

            if (!httpResponse.IsSuccessStatusCode)
            {
                if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException();
                }
                
                var message = await httpResponse.Content.ReadAsStringAsync();
                var error = JsonConvert.DeserializeObject<HttpError>(message);

                throw new HttpClientServiceException(httpResponse.StatusCode, error.Message);
            }
        }

        public void SetDefaultRequestHeader(string key, string value)
        {
            _defaultRequestHeaders[key] = value;
        }

        public void ClearDefaultRequestHeaders()
        {
            _defaultRequestHeaders.Clear();
        }

        private HttpClient GetClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout =TimeSpan.FromSeconds(90);

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            foreach (var defaultRequestHeader in _defaultRequestHeaders)
            {
                client.DefaultRequestHeaders.Add(defaultRequestHeader.Key, defaultRequestHeader.Value);
            }
            
            return client;
        }
    }
}