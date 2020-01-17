using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Auth.Abstractions.ClientAuthenticationHandlers;
using Blauhaus.HttpClientService.Abstractions;
using Blauhaus.HttpClientService.Abstractions.Exceptions;
using Blauhaus.HttpClientService.Config;
using Blauhaus.HttpClientService.Request;
using Blauhaus.Loggers.Common.Abstractions;
using Newtonsoft.Json;
using Polly;

namespace Blauhaus.HttpClientService.Service
{
    public class HttpClientService : IHttpClientService
    {
        private readonly IHttpClientServiceConfig _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogService _logService;
        private readonly IAuthenticatedAccessToken _defaultAccessToken;
        private readonly Dictionary<string, string> _defaultRequestHeaders = new Dictionary<string, string>();

        public HttpClientService(
            IHttpClientServiceConfig config, 
            IHttpClientFactory httpClientFactory, 
            ILogService logService,
            IAuthenticatedAccessToken accessToken)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
            _logService = logService;
            _defaultAccessToken = accessToken;
        }



        public async Task<TResponse> PostAsync<TRequest, TResponse>(string route, TRequest dto, CancellationToken token)
        {
            var httpClient = GetClient(new Dictionary<string, string>(), new KeyValuePair<string, string>());
            var httpContent = new StringContent(JsonConvert.SerializeObject(dto), new UTF8Encoding(), "application/json");

            var responseMessage = await TryExecuteAsync(t=> httpClient.PostAsync(route, httpContent, t), TimeSpan.FromSeconds(60), token);

            if (responseMessage.IsSuccessStatusCode)
            {
                return await UnwrapResponseAsync<TResponse>(responseMessage);
            }

            await HandleFailResponseAsync(responseMessage);
            return default;
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(IHttpRequestWrapper<TRequest> request, CancellationToken token)
        {
            var httpContent = new StringContent(JsonConvert.SerializeObject(request.Request), new UTF8Encoding(), "application/json");
            var client = GetClient(request.RequestHeaders, request.AuthorizationHeader);

            var httpResponse = await TryExecuteAsync(t=> client.PostAsync(request.Url, httpContent, t), TimeSpan.FromSeconds(60), token);

            if (!httpResponse.IsSuccessStatusCode)
            {
                await HandleFailResponseAsync(httpResponse);
            }
            
            return await UnwrapResponseAsync<TResponse>(httpResponse);
        }

        public async Task PostAsync<TRequest>(string route, TRequest dto, CancellationToken token)
        {
            var httpContent = new StringContent(JsonConvert.SerializeObject(dto), new UTF8Encoding(), "application/json");
            var httpClient = GetClient(new Dictionary<string, string>(), new KeyValuePair<string, string>());
            
            var httpResponse = await TryExecuteAsync(t=> httpClient.PostAsync(route, httpContent, t), TimeSpan.FromSeconds(60), token);

            if (!httpResponse.IsSuccessStatusCode)
            {
                await HandleFailResponseAsync(httpResponse);
            }
        }

        public  async Task<TResponse> PatchAsync<TRequest, TResponse>(IHttpRequestWrapper<TRequest> request, CancellationToken token)
        {
            var httpContent = new StringContent(JsonConvert.SerializeObject(request.Request), new UTF8Encoding(), "application/json");
            var requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), request.Url) {Content = httpContent};
            var client = GetClient(request.RequestHeaders, request.AuthorizationHeader);

            var httpResponse = await TryExecuteAsync(t=> client.SendAsync(requestMessage, t), TimeSpan.FromSeconds(60), token);

            if (!httpResponse.IsSuccessStatusCode)
            {
                await HandleFailResponseAsync(httpResponse);
            }
            
            return await UnwrapResponseAsync<TResponse>(httpResponse);
        }

        public async Task<TResponse> GetAsync<TResponse>(IHttpRequestWrapper request, CancellationToken token)
        {
            var client = GetClient(request.RequestHeaders, request.AuthorizationHeader);

            var httpResponse = await TryExecuteAsync(t => client.GetAsync(request.Url, t), TimeSpan.FromSeconds(60), token);

            if (!httpResponse.IsSuccessStatusCode)
            {
                await HandleFailResponseAsync(httpResponse);
            }
            
            return await UnwrapResponseAsync<TResponse>(httpResponse);
        }

        public async Task<TResponse> DeleteAsync<TResponse>(IHttpRequestWrapper request, CancellationToken token)
        {
            var client = GetClient(request.RequestHeaders, request.AuthorizationHeader);

            var httpResponse = await TryExecuteAsync(t => client.DeleteAsync(request.Url, t), TimeSpan.FromSeconds(60), token);

            if (!httpResponse.IsSuccessStatusCode)
            {
                await HandleFailResponseAsync(httpResponse);
            }
            
            return await UnwrapResponseAsync<TResponse>(httpResponse);
        }
        
        private async Task<HttpResponseMessage> TryExecuteAsync(Func<CancellationToken, Task<HttpResponseMessage>> task, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return await task.Invoke(cancellationToken);
        }

        //this is not used for now because it throws cancelled exceptions out and i don't know what type of HttpResponse message to return if the task is cancelled
        //too fucking complicated
        private async Task<HttpResponseMessage> TryExecuteWithTimeoutHandlerAsync(Func<CancellationToken, Task<HttpResponseMessage>> task, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return await Policy<HttpResponseMessage>
                .Handle<TimeoutException>(ex =>
                {
                    //_logService.Trace("Request Timed out");
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
        
        public void SetDefaultRequestHeader(string key, string value)
        {
            _defaultRequestHeaders[key] = value;
        }

        public void ClearDefaultRequestHeaders()
        {
            _defaultRequestHeaders.Clear();
        }
        

        #region Privates
        
        private async Task<TResponse> UnwrapResponseAsync<TResponse>(HttpResponseMessage responseMessage)
        {
            var jsonBody = await responseMessage.Content.ReadAsStringAsync();
            var deserializedResponse = JsonConvert.DeserializeObject<TResponse>(jsonBody);
            return deserializedResponse;
        }

        private async Task HandleFailResponseAsync(HttpResponseMessage httpResponse)
        {
            if (httpResponse.StatusCode == HttpStatusCode.Forbidden ||
                httpResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                //_logService.Trace($"Client is not allowed to access this endpoint!");
                throw new HttpClientServiceAuthorizationException(httpResponse.StatusCode, httpResponse.ReasonPhrase);
            }

            var message = await httpResponse.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<HttpError>(message);

            if (error != null && !string.IsNullOrEmpty(error.Message))
            {
                //_logService.Trace($"Server returned an error: {error.Message}");
                throw new HttpClientServiceServerError(httpResponse.StatusCode, error.Message);
            }
            //_logService.Trace($"General http client exception: {httpResponse.StatusCode} ({httpResponse.ReasonPhrase})");
            throw new HttpClientServiceException(httpResponse.StatusCode, httpResponse.ReasonPhrase);
            
        }


        private HttpClient GetClient(Dictionary<string, string> requestHeaders, KeyValuePair<string, string> authorizationHeader)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout =TimeSpan.FromSeconds(90);

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

            if (!string.IsNullOrEmpty(authorizationHeader.Key))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authorizationHeader.Key, authorizationHeader.Value);
            
            else if (!string.IsNullOrEmpty(_defaultAccessToken.Scheme))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_defaultAccessToken.Scheme, _defaultAccessToken.Token);
                

            return client;
        }
        


        #endregion
    }
}