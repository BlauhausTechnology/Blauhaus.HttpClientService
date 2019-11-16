using Blauhaus.HttpClientService.Request._Base;

namespace Blauhaus.HttpClientService.Request
{

    public class HttpRequestWrapper<TRequest> : BaseHttpRequestWrapper<HttpRequestWrapper<TRequest>>, IHttpRequestWrapper<TRequest>
    {
        public HttpRequestWrapper(string endpoint, TRequest request) 
            : base(endpoint)
        {
            Request = request;
        }

        public TRequest Request { get; }
    }

    public class HttpRequestWrapper : BaseHttpRequestWrapper<HttpRequestWrapper>
    {
        public HttpRequestWrapper(string endpoint) : base(endpoint)
        {
        }
    }
}