using System.Threading;
using System.Threading.Tasks;
using HttpClientService.Core.Request;

namespace HttpClientService.Core.Service
{
    public interface IHttpClientService
    {
        Task<TResponse> PostAsync<TRequest, TResponse>(string route, TRequest request, CancellationToken token);
        Task<TResponse> PostAsync<TRequest, TResponse>(IHttpRequestWrapper<TRequest> request, CancellationToken token);
        Task PostAsync<TRequest>(string route, TRequest request, CancellationToken token);

        void SetDefaultRequestHeader(string key, string value);
        void ClearDefaultRequestHeaders();
    }
}