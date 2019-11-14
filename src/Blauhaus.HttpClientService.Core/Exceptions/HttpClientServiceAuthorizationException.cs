using System.Net;

namespace HttpClientService.Core.Exceptions
{
    public class HttpClientServiceAuthorizationException : HttpClientServiceException
    {
        public HttpClientServiceAuthorizationException(HttpStatusCode httpStatusCode, string httpResponseReasonPhrase) 
            : base(httpStatusCode, httpResponseReasonPhrase)
        {
        }

    }
}