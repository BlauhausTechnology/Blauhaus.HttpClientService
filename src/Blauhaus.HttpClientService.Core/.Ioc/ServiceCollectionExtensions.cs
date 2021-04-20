using System.Diagnostics;
using Blauhaus.Analytics.Console.Ioc;
using Blauhaus.Auth.Abstractions.AccessToken;
using Blauhaus.Auth.Abstractions.Ioc;
using Blauhaus.HttpClientService.Abstractions;
using Blauhaus.HttpClientService.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Blauhaus.HttpClientService.Ioc
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddClientHttpService(this IServiceCollection services) 
        {
            services.TryAddSingleton<IAuthenticatedAccessToken, AuthenticatedAccessToken>();
            AddHttpService(services);
            return services;
        }

        public static IServiceCollection AddServerHttpService(this IServiceCollection services, TraceListener traceListener) 
        {
            services.TryAddScoped<IAuthenticatedAccessToken, AuthenticatedAccessToken>();
            AddHttpService(services);
            return services;
        }

        public static IServiceCollection AddHttpService(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddScoped<IHttpClientService, Service.HttpClientService>();
            services.AddScoped<IHttpClientServiceConfig, DefaultHttpClientServiceConfig>();
            return services;
        }
    }
}