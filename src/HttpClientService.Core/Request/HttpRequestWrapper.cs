using System.Collections.Generic;
using HttpClientService.Core.Request._Base;

namespace HttpClientService.Core.Request
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