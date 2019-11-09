using HttpClient.Core.Request._Base;

namespace HttpClient.Core.Request
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


   
}