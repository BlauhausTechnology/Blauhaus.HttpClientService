using System.Net;

namespace HttpClient.Core.Exceptions
{
    public class HttpClientServiceAuthorizationException : HttpClientServiceException
    {
        public HttpClientServiceAuthorizationException(HttpStatusCode httpStatusCode, string httpResponseReasonPhrase) 
            : base(httpStatusCode, httpResponseReasonPhrase)
        {
        }

    }
}