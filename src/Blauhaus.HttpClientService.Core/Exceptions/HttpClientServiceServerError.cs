﻿using System.Net;

namespace Blauhaus.HttpClientService.Exceptions
{
    public class HttpClientServiceServerError : HttpClientServiceException
    {
        public HttpClientServiceServerError(HttpStatusCode httpStatusCode, string errorMessage) 
            : base(httpStatusCode, errorMessage)
        {
        }
    }
}