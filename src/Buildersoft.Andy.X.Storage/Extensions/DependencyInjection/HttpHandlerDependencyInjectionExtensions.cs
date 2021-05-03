using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Buildersoft.Andy.X.Storage.Extensions.DependencyInjection
{
    public static class HttpHandlerDependencyInjectionExtensions
    {
        public static IServiceCollection AddHttpClientHandler(this IServiceCollection services)
        {
            services.AddSingleton<HttpClientHandler>(provider =>
            {
                var httpClientHandler = new HttpClientHandler();
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (env == "Development")
                {
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                }
                return httpClientHandler;
            });

            return services;
        }
    }
}
