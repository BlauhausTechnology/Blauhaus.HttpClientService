using System.Net;

namespace HttpClient.Core.Exceptions
{
    public class HttpClientServiceServerError : HttpClientServiceException
    {
        public HttpClientServiceServerError(HttpStatusCode httpStatusCode, string errorMessage) 
            : base(httpStatusCode, errorMessage)
        {
        }
    }
}