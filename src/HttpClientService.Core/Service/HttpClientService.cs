using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HttpClientService.Core.Request;
using Newtonsoft.Json;

namespace HttpClientService.Core.Service
{
    public class HttpClientService : IHttpClientService
    {
        private readonly IHttpClientServiceConfig _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Dictionary<string, string> _defaultRequestHeaders = new Dictionary<string, string>();
        private AuthenticationHeaderValue _authHeader;

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
                await HandleFailResponseAsync(httpResponse);
            }
            
            var jsonBody = await httpResponse.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TResponse>(jsonBody);
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(IHttpRequestWrapper<TRequest> request, CancellationToken token)
        {
            var url = ExtractUrlFromWrapper(request);
            var httpContent = new StringContent(JsonConvert.SerializeObject(request.Request), new UTF8Encoding(), "application/json");
            var client = GetClient(request.RequestHeaders);
            
            var httpResponse = await client.PostAsync(url, httpContent, token);

            if (!httpResponse.IsSuccessStatusCode)
            {
                await HandleFailResponseAsync(httpResponse);
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
                await HandleFailResponseAsync(httpResponse);
            }
        }

        private static async Task HandleFailResponseAsync(HttpResponseMessage httpResponse)
        {
            if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException();
            }

            var message = await httpResponse.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<HttpError>(message);
            var errorMessage = error == null ? string.Empty : error.Message;
            throw new HttpClientServiceException(httpResponse.StatusCode, errorMessage);
        }

        public void SetDefaultRequestHeader(string key, string value)
        {
            _defaultRequestHeaders[key] = value;
        }

        public void ClearDefaultRequestHeaders()
        {
            _defaultRequestHeaders.Clear();
        }

        private HttpClient GetClient(Dictionary<string, string> requestHeaders = null)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout =TimeSpan.FromSeconds(90);

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (requestHeaders == null || requestHeaders.Count == 0)
            {
                foreach (var defaultRequestHeader in _defaultRequestHeaders)
                {
                    client.DefaultRequestHeaders.Add(defaultRequestHeader.Key, defaultRequestHeader.Value);
                }
            }
            else
            {
                foreach (var requestHeader in requestHeaders)
                {
                    client.DefaultRequestHeaders.Add(requestHeader.Key, requestHeader.Value);
                }
            }

            client.DefaultRequestHeaders.Authorization = _authHeader;
            
            return client;
        }
        
        private static string ExtractUrlFromWrapper(IHttpRequestWrapper request)
        {
            var url = new StringBuilder(request.Endpoint);
            if (request.QueryStringParameters.Count > 0)
            {
                url.Append("?");
                foreach (var parameter in request.QueryStringParameters)
                {
                    url.Append(parameter.Key).Append("=").Append(parameter.Value).Append("&");
                }

                url.Length -= 1;
            }

            return url.ToString();
        }

        public void HandleAccessToken(string authenticatedAccessToken)
        {
            _authHeader = new AuthenticationHeaderValue("Bearer", authenticatedAccessToken);
        }


        public void ClearAccessToken()
        {
            _authHeader = new AuthenticationHeaderValue("Bearer", String.Empty);
        }
    }
}