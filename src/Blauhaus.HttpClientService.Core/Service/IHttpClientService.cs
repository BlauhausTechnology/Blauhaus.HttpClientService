﻿using System.Threading;
using System.Threading.Tasks;
using Blauhaus.Auth.Abstractions.ClientAuthenticationHandlers;
using Blauhaus.HttpClientService.Request;

namespace Blauhaus.HttpClientService.Service
{
    public interface IHttpClientService : IAuthenticatedAccessTokenHandler
    {
        Task<TResponse> PostAsync<TRequest, TResponse>(string route, TRequest request, CancellationToken token);
        Task<TResponse> PostAsync<TRequest, TResponse>(IHttpRequestWrapper<TRequest> request, CancellationToken token);
        Task PostAsync<TRequest>(string route, TRequest request, CancellationToken token);

        void SetDefaultRequestHeader(string key, string value);
        void ClearDefaultRequestHeaders();
    }
}