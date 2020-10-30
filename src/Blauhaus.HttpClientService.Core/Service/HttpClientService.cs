using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Analytics.Abstractions.Service;
using Blauhaus.Auth.Abstractions.AccessToken;
using Blauhaus.HttpClientService.Abstractions;
using Blauhaus.HttpClientService.Abstractions.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;

namespace Blauhaus.HttpClientService.Service
{
    public class HttpClientService : IHttpClientService
    {
        private readonly IHttpClientServiceConfig _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAnalyticsService _analyticsService;
        private readonly IAuthenticatedAccessToken _defaultAccessToken;
        private readonly Dictionary<string, string> _defaultRequestHeaders = new Dictionary<string, string>();
        private readonly TimeSpan _timeout;

        public HttpClientService(
            IHttpClientServiceConfig config, 
            IHttpClientFactory httpClientFactory, 
            IAnalyticsService analyticsService,
            IAuthenticatedAccessToken accessToken)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
            _analyticsService = analyticsService;
            _defaultAccessToken = accessToken;
            _timeout = config.RequestTimeout ?? TimeSpan.FromMinutes(1);
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string route, TRequest dto, CancellationToken token)
        {
            var start = DateTime.Now;

            var httpClient = GetClient(new Dictionary<string, string>(), new KeyValuePair<string, string>());
            var httpContent = new StringContent(JsonConvert.SerializeObject(dto), new UTF8Encoding(), "application/json");

            var responseMessage = await TryExecuteAsync(t=> httpClient.PostAsync(route, httpContent, t), _timeout, token);

            if (responseMessage.IsSuccessStatusCode)
            {
                return await UnwrapResponseAsync<TResponse>(responseMessage, start);
            }

            await HandleFailResponseAsync(responseMessage, start);
            return default;
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(IHttpRequestWrapper<TRequest> request, CancellationToken token)
        {
            var start = DateTime.Now;

            var httpContent = new StringContent(JsonConvert.SerializeObject(request.Request), new UTF8Encoding(), "application/json");
            var client = GetClient(request.RequestHeaders, request.AuthorizationHeader);

            var httpResponse = await TryExecuteAsync(t=> client.PostAsync(request.Url, httpContent, t), _timeout, token);

            if (!httpResponse.IsSuccessStatusCode)
            {
                await HandleFailResponseAsync(httpResponse, start);
            }
            
            return await UnwrapResponseAsync<TResponse>(httpResponse, start);
        }

        public async Task PostAsync<TRequest>(string route, TRequest dto, CancellationToken token)
        {
            var start = DateTime.Now;

            var httpContent = new StringContent(JsonConvert.SerializeObject(dto), new UTF8Encoding(), "application/json");
            var httpClient = GetClient(new Dictionary<string, string>(), new KeyValuePair<string, string>());
            
            var httpResponse = await TryExecuteAsync(t=> httpClient.PostAsync(route, httpContent, t), _timeout, token);

            if (!httpResponse.IsSuccessStatusCode)
            {
                await HandleFailResponseAsync(httpResponse, start);
            }
        }

        public  async Task<TResponse> PatchAsync<TResponse>(IHttpRequestWrapper<JObject> request, CancellationToken token)
        {
            var start = DateTime.Now;

            var httpContent = new StringContent(request.Request.ToString(), new UTF8Encoding(), "application/json");
            var requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), request.Url) {Content = httpContent};
            var client = GetClient(request.RequestHeaders, request.AuthorizationHeader);

            var httpResponse = await TryExecuteAsync(t=> client.SendAsync(requestMessage, t), TimeSpan.FromSeconds(60), token);

            if (!httpResponse.IsSuccessStatusCode)
            {
                await HandleFailResponseAsync(httpResponse, start);
            }
            
            return await UnwrapResponseAsync<TResponse>(httpResponse, start);
        }

        public async Task<TResponse> GetAsync<TResponse>(IHttpRequestWrapper request, CancellationToken token)
        {
            var start = DateTime.Now;

            var client = GetClient(request.RequestHeaders, request.AuthorizationHeader);

            var httpResponse = await TryExecuteAsync(t => client.GetAsync(request.Url, t), _timeout, token);

            if (!httpResponse.IsSuccessStatusCode)
            {
                await HandleFailResponseAsync(httpResponse, start);
            }
            
            return await UnwrapResponseAsync<TResponse>(httpResponse, start);
        }

        public async Task<TResponse> DeleteAsync<TResponse>(IHttpRequestWrapper request, CancellationToken token)
        {
            var start = DateTime.Now;

            var client = GetClient(request.RequestHeaders, request.AuthorizationHeader);

            var httpResponse = await TryExecuteAsync(t => client.DeleteAsync(request.Url, t), _timeout, token);

            if (!httpResponse.IsSuccessStatusCode)
            {
                await HandleFailResponseAsync(httpResponse, start);
            }
            
            return await UnwrapResponseAsync<TResponse>(httpResponse, start);
        }
        
        private async Task<HttpResponseMessage> TryExecuteAsync(Func<CancellationToken, Task<HttpResponseMessage>> task, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return await task.Invoke(cancellationToken);
        }

        public void SetDefaultRequestHeader(string key, string value)
        {
            _defaultRequestHeaders[key] = value;
        }

        public bool TryGetDefaultRequestHeader(string key, out string value)
        {
            return _defaultRequestHeaders.TryGetValue(key, out value);
        }

        public void ClearDefaultRequestHeaders()
        {
            _defaultRequestHeaders.Clear();
        }
        

        #region Privates
        
        private async Task<TResponse> UnwrapResponseAsync<TResponse>(HttpResponseMessage httpResponse, DateTime start)
        {
            var jsonBody = await httpResponse.Content.ReadAsStringAsync();
            var deserializedResponse = JsonConvert.DeserializeObject<TResponse>(jsonBody);
            
            var trace = new StringBuilder().Append("HttpClientService: ");

            if (httpResponse.RequestMessage != null)
            {
                trace.Append(httpResponse.RequestMessage.Method.Method)
                    .Append(" to ")
                    .Append(httpResponse.RequestMessage.RequestUri.Host);
            }
            else
            {
                trace.Append("Request");
            }

            trace.Append(" succeeded with " + httpResponse.StatusCode);
            trace.Append(" in " + (DateTime.Now - start).TotalMilliseconds + "ms");

            _analyticsService.Trace(this, trace.ToString());

            return deserializedResponse;
        }

        private async Task HandleFailResponseAsync(HttpResponseMessage httpResponse, DateTime start)
        {
            var traceProperties = new Dictionary<string, object>
            {
                {"Http.StatusCode",  httpResponse.StatusCode},
                {"Http.ReasonPhrase",  httpResponse.ReasonPhrase},
                {"Http.RequestUri",  httpResponse?.RequestMessage?.RequestUri},
                {"Http.Method",  httpResponse?.RequestMessage?.Method},
            };

            if (httpResponse.StatusCode == HttpStatusCode.Forbidden ||
                httpResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                _analyticsService.Trace(this, $"HttpClientService: Request authorization failed", LogSeverity.Warning, traceProperties);
                throw new HttpClientServiceAuthorizationException(httpResponse.StatusCode, httpResponse.ReasonPhrase);
            }

            var message = await httpResponse.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<HttpError>(message);

            if (error != null && !string.IsNullOrEmpty(error.Message))
            {
                traceProperties["ServerErrorMessage"] = error.Message;
                _analyticsService.Trace(this, "Server error", LogSeverity.Warning, traceProperties);
                throw new HttpClientServiceServerError(httpResponse.StatusCode, error.Message);
            }

            _analyticsService.Trace(this, "HttpClientService: HttpClient error in " + (DateTime.Now - start).TotalMilliseconds + "ms", LogSeverity.Information, traceProperties);
            throw new HttpClientServiceException(httpResponse.StatusCode, httpResponse.ReasonPhrase);
        }


        public HttpClient GetClient(Dictionary<string, string> requestHeaders = default, KeyValuePair<string, string> authorizationHeader = default)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = _timeout;

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            foreach (var defaultRequestHeader in _defaultRequestHeaders)
            {
                client.DefaultRequestHeaders.Add(defaultRequestHeader.Key, defaultRequestHeader.Value);
            }
            if(requestHeaders!=null)
            {
                foreach (var requestHeader in requestHeaders)
                {
                    client.DefaultRequestHeaders.Add(requestHeader.Key, requestHeader.Value);
                }
            }

            foreach (var additionalHeader in _defaultAccessToken.AdditionalHeaders)
            {
                client.DefaultRequestHeaders.Add(additionalHeader.Key, additionalHeader.Value);
            }

            foreach (var analyticsHeader in _analyticsService.AnalyticsOperationHeaders)
            {
                client.DefaultRequestHeaders.Add(analyticsHeader.Key, analyticsHeader.Value);
            }

            if (!string.IsNullOrEmpty(authorizationHeader.Key))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authorizationHeader.Key, authorizationHeader.Value);
            
            else if (!string.IsNullOrEmpty(_defaultAccessToken.Scheme))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_defaultAccessToken.Scheme, _defaultAccessToken.Token);
                

            return client;
        }
        


        #endregion
    }
}