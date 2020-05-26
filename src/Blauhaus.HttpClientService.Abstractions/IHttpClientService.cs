using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Blauhaus.HttpClientService.Abstractions
{
    public interface IHttpClientService
    {
        [Obsolete("Use IHttpRequestWrapper instead")]
        Task<TResponse> PostAsync<TRequest, TResponse>(string route, TRequest request, CancellationToken token);
        [Obsolete("Use IHttpRequestWrapper instead")]
        Task PostAsync<TRequest>(string route, TRequest request, CancellationToken token);
        
        Task<TResponse> PostAsync<TRequest, TResponse>(IHttpRequestWrapper<TRequest> request, CancellationToken token);
        Task<TResponse> PatchAsync<TResponse>(IHttpRequestWrapper<JObject> request, CancellationToken token);
        Task<TResponse> GetAsync<TResponse>(IHttpRequestWrapper request, CancellationToken token);
        Task<TResponse> DeleteAsync<TResponse>(IHttpRequestWrapper request, CancellationToken token);

        HttpClient GetClient(Dictionary<string, string> requestHeaders = default, KeyValuePair<string, string> authorizationHeader = default);

        void SetDefaultRequestHeader(string key, string value);
        bool TryGetDefaultRequestHeader(string key, out string value);
        void ClearDefaultRequestHeaders();
    }
}