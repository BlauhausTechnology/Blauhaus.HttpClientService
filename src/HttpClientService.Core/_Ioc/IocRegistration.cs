﻿using System.Net.Http;
using Blauhaus.Ioc.Abstractions;
using HttpClient.Core.Config;
using HttpClient.Core.Service;
using Microsoft.Extensions.DependencyInjection;

namespace HttpClient.Core._Ioc
{
    public static class IocRegistration
    {
        public static IIocService RegisterHttpService(this IIocService iocService) 
        {
            var httpClientFactory = (IHttpClientFactory)new ServiceCollection()
                .AddHttpClient()
                .BuildServiceProvider()
                .GetService(typeof(IHttpClientFactory));
            iocService.RegisterInstance(httpClientFactory);
            iocService.RegisterImplementation<IHttpClientService, Service.HttpClientService>(IocLifetime.Singleton);
            iocService.RegisterImplementation<IHttpClientServiceConfig, DefaultHttpClientServiceConfig>(IocLifetime.Singleton);
            return iocService;
        }
    }
}