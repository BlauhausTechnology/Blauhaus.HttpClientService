using System.Collections.Generic;

namespace HttpClientService.Core.Request._Base
{
    public abstract class BaseHttpRequestWrapper<TWrapper> : IHttpRequestWrapper where TWrapper : BaseHttpRequestWrapper<TWrapper>
    {
        protected BaseHttpRequestWrapper(string endpoint)
        {
            Endpoint = endpoint;
        }

        public string Endpoint { get; }
        public Dictionary<string, string> RequestHeaders { get; } = new Dictionary<string, string>();
        public Dictionary<string, string> QueryStringParameters { get; } = new Dictionary<string, string>();

        public TWrapper WithRequestHeader(string key, string vale)
        {
            RequestHeaders[key] = vale;
            return this as TWrapper;
        }
        
        public TWrapper WithQueryStringParameter(string key, string vale)
        {
            QueryStringParameters[key] = vale;
            return this as TWrapper;
        }

    }
}