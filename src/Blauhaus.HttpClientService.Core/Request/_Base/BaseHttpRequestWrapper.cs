using System.Collections.Generic;
using System.Text;

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

        public string Url
        {
            get
            {
                var url = new StringBuilder(Endpoint);
                if (QueryStringParameters.Count > 0)
                {
                    url.Append("?");
                    foreach (var parameter in QueryStringParameters)
                    {
                        url.Append(parameter.Key).Append("=").Append(parameter.Value).Append("&");
                    }

                    url.Length -= 1;
                }

                return url.ToString();
            }
        }

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

        public TWrapper WithAuthorizationHeader(string scheme, string token)
        {
            AuthorizationHeader = new KeyValuePair<string, string>(scheme, token);
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