using System.Net;

namespace Blauhaus.HttpClientService.Exceptions
{
    public class HttpClientServiceAuthorizationException : HttpClientServiceException
    {
        public HttpClientServiceAuthorizationException(HttpStatusCode httpStatusCode, string httpResponseReasonPhrase) 
            : base(httpStatusCode, httpResponseReasonPhrase)
        {
        }

    }
}