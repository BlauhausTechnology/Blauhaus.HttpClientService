using System.Net;

namespace Blauhaus.HttpClientService.Abstractions.Exceptions
{
    public class HttpClientServiceAuthorizationException : HttpClientServiceException
    {
        public HttpClientServiceAuthorizationException(HttpStatusCode httpStatusCode, string httpResponseReasonPhrase) 
            : base(httpStatusCode, httpResponseReasonPhrase)
        {
        }

    }
}