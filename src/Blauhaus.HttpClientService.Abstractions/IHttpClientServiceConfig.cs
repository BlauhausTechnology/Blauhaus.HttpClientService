using System;

namespace Blauhaus.HttpClientService.Abstractions
{
    public interface IHttpClientServiceConfig
    {
        TimeSpan? RequestTimeout { get; }
    }
}