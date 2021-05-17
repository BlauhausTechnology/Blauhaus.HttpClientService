using System;
using System.Net;

namespace Blauhaus.Http.Abstractions.Errors
{
    public class HttpResponseException : Exception
    {
        public HttpResponseException(HttpStatusCode statusCode,  string method, string uri, string content) 
            : base($"{statusCode}: {content}")
        {
            StatusCode = statusCode;
            Method = method;
            Uri = uri;
        }

        public HttpStatusCode StatusCode { get; }
        public string Method { get; }
        public string Uri { get; }
    }
}