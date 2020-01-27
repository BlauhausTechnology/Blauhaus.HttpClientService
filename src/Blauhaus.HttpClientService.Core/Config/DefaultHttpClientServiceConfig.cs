using System;
using Blauhaus.HttpClientService.Abstractions;

namespace Blauhaus.HttpClientService.Config
{
    public class DefaultHttpClientServiceConfig : IHttpClientServiceConfig
    {
        public DefaultHttpClientServiceConfig()
        {
            RequestTimeout = TimeSpan.FromMinutes(1);
        }

        public TimeSpan? RequestTimeout { get; }
    }
}