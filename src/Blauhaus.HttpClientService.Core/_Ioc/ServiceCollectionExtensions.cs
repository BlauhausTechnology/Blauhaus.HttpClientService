using System.Diagnostics;
using Blauhaus.Analytics.Console._Ioc;
using Blauhaus.Auth.Abstractions._Ioc;
using Blauhaus.Auth.Abstractions.ClientAuthenticationHandlers;
using Blauhaus.HttpClientService.Abstractions;
using Blauhaus.HttpClientService.Config;
using Microsoft.Extensions.DependencyInjection;

namespace Blauhaus.HttpClientService._Ioc
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterServerHttpService(this IServiceCollection services, TraceListener traceListener) 
        {
            services.RegisterAccessToken();
            Register(services, traceListener);
            return services;
        }

        public static IServiceCollection RegisterServerHttpService<TAccessToken>(this IServiceCollection services, TraceListener traceListener) where TAccessToken : AuthenticatedAccessToken
        {
            services.RegisterAccessToken<TAccessToken>();
            Register(services, traceListener);
            return services;
        }

        private static void Register(IServiceCollection services, TraceListener traceListener)
        {
            services.AddHttpClient();
            services.AddScoped<IHttpClientService, Service.HttpClientService>();
            services.AddScoped<IHttpClientServiceConfig, DefaultHttpClientServiceConfig>();
            services.RegisterConsoleLoggerServerService(traceListener);
        }
    }
}