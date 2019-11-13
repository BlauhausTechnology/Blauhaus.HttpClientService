using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Loggers.Common.Abstractions;
using HttpClient.Core.Config;
using HttpClient.Core.Request;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using LogLevel = Blauhaus.Loggers.Common.Abstractions.LogLevel;

namespace HttpClient.Core.Service
{
    public class HttpClientService : IHttpClientService
    {
        private readonly IHttpClientServiceConfig _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogService _logService;
        private readonly Dictionary<string, string> _defaultRequestHeaders = new Dictionary<string, string>();
        private AuthenticationHeaderValue _authHeader;

        public HttpClientService(IHttpClientServiceConfig config, IHttpClientFactory httpClientFactory, ILogService logService)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
            _logService = logService;
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string route, TRequest dto, CancellationToken token)
        {
            var httpContent = new StringContent(JsonConvert.SerializeObject(dto), new UTF8Encoding(), "application/json");
            var httpResponse = await GetClient().PostAsync(route, httpContent, token);

            if (!httpResponse.IsSuccessStatusCode)
            {
                await HandleFailResponseAsync(httpResponse);
            }

            return await UnwrapResponseAsync<TRequest, TResponse>(httpResponse, route);
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
            
            return await UnwrapResponseAsync<TRequest, TResponse>(httpResponse, request.Endpoint);
        }

        public async Task PostAsync<TRequest>(string route, TRequest dto, CancellationToken token)
        {
            var httpContent = new StringContent(JsonConvert.SerializeObject(dto), new UTF8Encoding(), "application/json");
            var httpResponse = await GetClient().PostAsync(route, httpContent, token);

            if (!httpResponse.IsSuccessStatusCode)
            {
                await HandleFailResponseAsync(httpResponse);
            }
            _logService.LogMessage(LogLevel.Trace, $"Successfully posted {typeof(TRequest).Name} request to {route}", null);
        }

        private async Task<TResponse> UnwrapResponseAsync<TRequest, TResponse>(HttpResponseMessage responseMessage, string route)
        {
            var jsonBody = await responseMessage.Content.ReadAsStringAsync();
            var deserializedResponse = JsonConvert.DeserializeObject<TResponse>(jsonBody);
            _logService.LogMessage(LogLevel.Trace, $"Successfully posted {typeof(TRequest).Name} request to {route}", null);
            return deserializedResponse;
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

        private System.Net.Http.HttpClient GetClient(Dictionary<string, string> requestHeaders = null)
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