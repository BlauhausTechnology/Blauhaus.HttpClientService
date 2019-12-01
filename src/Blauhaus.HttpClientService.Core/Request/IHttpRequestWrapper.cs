using System.Collections.Generic;

namespace Blauhaus.HttpClientService.Request
{

    public interface IHttpRequestWrapper<out TRequest> : IHttpRequestWrapper
    {
        TRequest Request { get; }
    }

    public interface IHttpRequestWrapper
    {
        string Endpoint { get; }
        KeyValuePair<string, string> AuthorizationHeader { get; } 
        Dictionary<string, string> RequestHeaders { get; }
        Dictionary<string, string> QueryStringParameters { get; }

        string Url { get; }
    }
}