using System;
using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Auth.Abstractions.ClientAuthenticationHandlers;
using Blauhaus.HttpClientService.Request;

namespace Blauhaus.HttpClientService.Service
{
    public interface IHttpClientService
    {
        [Obsolete("Use IHttpRequestWrapper instead")]
        Task<TResponse> PostAsync<TRequest, TResponse>(string route, TRequest request, CancellationToken token);
        [Obsolete("Use IHttpRequestWrapper instead")]
        Task PostAsync<TRequest>(string route, TRequest request, CancellationToken token);
        
        Task<TResponse> PostAsync<TRequest, TResponse>(IHttpRequestWrapper<TRequest> request, CancellationToken token);
        Task<TResponse> PatchAsync<TRequest, TResponse>(IHttpRequestWrapper<TRequest> request, CancellationToken token);
        Task<TResponse> GetAsync<TResponse>(IHttpRequestWrapper request, CancellationToken token);
        Task<TResponse> DeleteAsync<TResponse>(IHttpRequestWrapper request, CancellationToken token);


        void SetDefaultRequestHeader(string key, string value);
        void ClearDefaultRequestHeaders();
    }
}