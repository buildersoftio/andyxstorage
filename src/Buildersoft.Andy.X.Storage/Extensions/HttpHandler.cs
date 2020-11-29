using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Buildersoft.Andy.X.Storage.Extensions
{
    public static class HttpHandler
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
