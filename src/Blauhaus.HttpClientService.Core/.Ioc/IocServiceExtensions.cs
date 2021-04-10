using System.Net.Http;
using Blauhaus.Analytics.Console.Ioc;
using Blauhaus.Auth.Abstractions.Ioc;
using Blauhaus.HttpClientService.Abstractions;
using Blauhaus.HttpClientService.Config;
using Blauhaus.Ioc.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Blauhaus.HttpClientService.Ioc
{
    public static class IocServiceExtensions
    {
        public static IIocService RegisterClientHttpService(this IIocService iocService) 
        {
            iocService.RegisterAccessToken();
            Register(iocService);
            return iocService;
        }

        private static void Register(IIocService iocService)
        {
            var httpClientFactory = (IHttpClientFactory)new ServiceCollection()
                .AddHttpClient()
                .BuildServiceProvider()
                .GetService(typeof(IHttpClientFactory));
            iocService.RegisterInstance(httpClientFactory);
            iocService.RegisterImplementation<IHttpClientService, Service.HttpClientService>(IocLifetime.Singleton);
            iocService.RegisterImplementation<IHttpClientServiceConfig, DefaultHttpClientServiceConfig>(IocLifetime.Singleton);
            iocService.RegisterConsoleLoggerClientService();
        }
    }
}