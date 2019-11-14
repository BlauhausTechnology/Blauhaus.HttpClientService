using System.Net;

namespace HttpClientService.Core.Exceptions
{
    public class HttpClientServiceServerError : HttpClientServiceException
    {
        public HttpClientServiceServerError(HttpStatusCode httpStatusCode, string errorMessage) 
            : base(httpStatusCode, errorMessage)
        {
        }
    }
}