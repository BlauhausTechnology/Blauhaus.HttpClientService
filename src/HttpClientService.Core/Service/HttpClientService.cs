using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Loggers.Common.Abstractions;
using Blauhaus.Loggers.Common.Extensions;
using HttpClient.Core.Config;
using HttpClient.Core.Exceptions;
using HttpClient.Core.Request;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
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

        public HttpClientService(
            IHttpClientServiceConfig config, 
            IHttpClientFactory httpClientFactory, 
            ILogService logService)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
            _logService = logService;
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string route, TRequest dto, CancellationToken token)
        {
            var httpClient = GetClient();
            var httpContent = new StringContent(JsonConvert.SerializeObject(dto), new UTF8Encoding(), "application/json");

            var responseMessage = await TryExecuteAsync(t=> httpClient.PostAsync(route, httpContent, t), TimeSpan.FromSeconds(60), token);

            if (responseMessage.IsSuccessStatusCode)
            {
                return await UnwrapResponseAsync<TRequest, TResponse>(responseMessage, route);
            }

            await HandleFailResponseAsync(responseMessage);
            return default;
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(IHttpRequestWrapper<TRequest> request, CancellationToken token)
        {
            var url = ExtractUrlFromWrapper(request);
            var httpContent = new StringContent(JsonConvert.SerializeObject(request.Request), new UTF8Encoding(), "application/json");
            var client = GetClient(request.RequestHeaders);

            var httpResponse = await TryExecuteAsync(t=> client.PostAsync(url, httpContent, t), TimeSpan.FromSeconds(60), token);

            if (!httpResponse.IsSuccessStatusCode)
            {
                await HandleFailResponseAsync(httpResponse);
            }
            
            return await UnwrapResponseAsync<TRequest, TResponse>(httpResponse, request.Endpoint);
        }

        public async Task PostAsync<TRequest>(string route, TRequest dto, CancellationToken token)
        {
            var httpContent = new StringContent(JsonConvert.SerializeObject(dto), new UTF8Encoding(), "application/json");
            var httpClient = GetClient();
            
            var httpResponse = await TryExecuteAsync(t=> httpClient.PostAsync(route, httpContent, t), TimeSpan.FromSeconds(60), token);

            if (!httpResponse.IsSuccessStatusCode)
            {
                await HandleFailResponseAsync(httpResponse);
            }
        }

        private async Task<HttpResponseMessage> TryExecuteAsync(Func<CancellationToken, Task<HttpResponseMessage>> task, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return await Policy<HttpResponseMessage>
                .Handle<TimeoutException>(ex =>
                {
                    _logService.Trace("Request Timed out");
                    return true;
                })
                .WaitAndRetryAsync
                (
                    5,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                )
                .ExecuteAsync(async () =>
                {
                    //httpclient throws TaskCanceled for timeouts and cancellations, so this method creates a custom timeout

                    var cts = new CancellationTokenSource();
                    cts.CancelAfter(timeout);
                    var timeoutToken = cts.Token;

                    var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken);

                    try
                    {
                        return await task.Invoke(linkedToken.Token);
                    }
                    catch (OperationCanceledException) when (timeoutToken.IsCancellationRequested)
                    {
                        throw new TimeoutException();
                    }
                });
        }

        private async Task<TResponse> UnwrapResponseAsync<TRequest, TResponse>(HttpResponseMessage responseMessage, string route)
        {
            var jsonBody = await responseMessage.Content.ReadAsStringAsync();
            var deserializedResponse = JsonConvert.DeserializeObject<TResponse>(jsonBody);
            _logService.LogMessage(LogLevel.Trace, $"Successfully posted {typeof(TRequest).Name} request to {route}");
            return deserializedResponse;
        }

        private async Task HandleFailResponseAsync(HttpResponseMessage httpResponse)
        {
            if (httpResponse.StatusCode == HttpStatusCode.Forbidden ||
                httpResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logService.Trace($"Client is not allowed to access this endpoint!");
                throw new HttpClientServiceAuthorizationException(httpResponse.StatusCode, httpResponse.ReasonPhrase);
            }

            var message = await httpResponse.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<HttpError>(message);

            if (error != null && string.IsNullOrEmpty(error.Message))
            {
                _logService.Trace($"Server returned an error: {error.Message}");
                throw new HttpClientServiceServerError(httpResponse.StatusCode, error.Message);
            }
            _logService.Trace($"General http client exception: {httpResponse.StatusCode} ({httpResponse.ReasonPhrase})");
            throw new HttpClientServiceException(httpResponse.StatusCode, httpResponse.ReasonPhrase);
            
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
            _authHeader = new AuthenticationHeaderValue("Bearer", string.Empty);
        }
    }
}