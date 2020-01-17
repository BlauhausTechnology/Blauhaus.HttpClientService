using System.Net.Http;
using Blauhaus.Auth.Abstractions._Ioc;
using Blauhaus.Auth.Abstractions.ClientAuthenticationHandlers;
using Blauhaus.HttpClientService.Abstractions;
using Blauhaus.HttpClientService.Config;
using Blauhaus.HttpClientService.Service;
using Blauhaus.Ioc.Abstractions;
using Blauhaus.Loggers.Console._Ioc;
using Microsoft.Extensions.DependencyInjection;

namespace Blauhaus.HttpClientService._Ioc
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterHttpService(this IServiceCollection services) 
        {
            services.RegisterAccessToken();
            Register(services);
            return services;
        }

        public static IServiceCollection RegisterHttpService<TAccessToken>(this IServiceCollection services) where TAccessToken : AuthenticatedAccessToken
        {
            services.RegisterAccessToken<TAccessToken>();
            Register(services);
            return services;
        }

        private static void Register(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddSingleton<IHttpClientService, Service.HttpClientService>();
            services.AddSingleton<IHttpClientServiceConfig, DefaultHttpClientServiceConfig>();
            services.RegisterConsoleLogger();
        }
    }
}