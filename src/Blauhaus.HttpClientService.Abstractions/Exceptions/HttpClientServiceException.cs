﻿using System;
using System.Net;

namespace Blauhaus.HttpClientService.Abstractions.Exceptions
{
    public class HttpClientServiceException : Exception
    {
        public HttpClientServiceException(HttpStatusCode httpStatusCode, string httpResponseReasonPhrase)
        {
            HttpStatusCode = httpStatusCode;
            HttpResponseReasonPhrase = httpResponseReasonPhrase;
        }

        public HttpStatusCode HttpStatusCode { get; }
        public string HttpResponseReasonPhrase { get; }
    }
}