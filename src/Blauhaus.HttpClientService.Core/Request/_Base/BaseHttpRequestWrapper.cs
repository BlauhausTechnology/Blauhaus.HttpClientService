using System.Collections.Generic;

namespace Blauhaus.HttpClientService.Request._Base
{




    public abstract class BaseHttpRequestWrapper<TWrapper> : IHttpRequestWrapper 
        where TWrapper : BaseHttpRequestWrapper<TWrapper>
    {
        protected BaseHttpRequestWrapper(string endpoint)
        {
            Endpoint = endpoint;
        }

        public string Endpoint { get; }
        public KeyValuePair<string, string> AuthorizationHeader { get; private set; } = new KeyValuePair<string, string>("", "");
        public Dictionary<string, string> RequestHeaders { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string, string> QueryStringParameters { get; private set;} = new Dictionary<string, string>();

        public TWrapper WithRequestHeader(string key, string vale)
        {
            RequestHeaders[key] = vale;
            return this as TWrapper;
        }
        public TWrapper WithRequestHeaders(Dictionary<string, string> headers)
        {
            RequestHeaders = headers;
            return this as TWrapper;
        }

        public TWrapper WithAuthorizationHeader(string token, string scheme = "Bearer")
        {
            AuthorizationHeader = new KeyValuePair<string, string>(token, scheme);
            return this as TWrapper;
        }

        public TWrapper WithQueryStringParameter(string key, string vale)
        {
            QueryStringParameters[key] = vale;
            return this as TWrapper;
        }
        
        public TWrapper WithQueryStringParameters(Dictionary<string, string> parameters)
        {
            QueryStringParameters = parameters;
            return this as TWrapper;
        }
    }
}