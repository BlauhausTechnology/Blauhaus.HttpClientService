using System.Diagnostics;
using Blauhaus.Analytics.Console.Ioc;
using Blauhaus.Auth.Abstractions.AccessToken;
using Blauhaus.Auth.Abstractions.Ioc;
using Blauhaus.HttpClientService.Abstractions;
using Blauhaus.HttpClientService.Config;
using Microsoft.Extensions.DependencyInjection;

namespace Blauhaus.HttpClientService.Ioc
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterClientHttpService(this IServiceCollection services) 
        {
            services.RegisterAccessToken();
            services.RegisterConsoleLoggerClientService();
            Register(services);
            return services;
        }

        public static IServiceCollection RegisterServerHttpService(this IServiceCollection services, TraceListener traceListener) 
        {
            services.RegisterAccessToken();
            //services.RegisterConsoleLoggerService(traceListener);
            Register(services);
            return services;
        }

        public static IServiceCollection RegisterServerHttpService<TAccessToken>(this IServiceCollection services, TraceListener traceListener) where TAccessToken : AuthenticatedAccessToken
        {
            services.RegisterAccessToken();
            //.RegisterConsoleLoggerService(traceListener);
            Register(services);
            return services;
        }

        private static void Register(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddScoped<IHttpClientService, Service.HttpClientService>();
            services.AddScoped<IHttpClientServiceConfig, DefaultHttpClientServiceConfig>();
        }
    }
}