using System.Net.Http;
using Blauhaus.Auth.Abstractions._Ioc;
using Blauhaus.Auth.Abstractions.ClientAuthenticationHandlers;
using Blauhaus.HttpClientService.Config;
using Blauhaus.HttpClientService.Service;
using Blauhaus.Ioc.Abstractions;
using Blauhaus.Loggers.Console._Ioc;
using Microsoft.Extensions.DependencyInjection;

namespace Blauhaus.HttpClientService._Ioc
{
    public static class IocRegistration
    {
        public static IIocService RegisterHttpService(this IIocService iocService) 
        {
            iocService.RegisterAccessToken();
            Register(iocService);
            return iocService;
        }

        public static IIocService RegisterHttpService<TAccessToken>(this IIocService iocService) where TAccessToken : AuthenticatedAccessToken
        {
            iocService.RegisterAccessToken<TAccessToken>();
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
            iocService.RegisterConsoleLogger();
        }
    }
}