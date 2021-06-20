using Buildersoft.Andy.X.Storage.IO.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Buildersoft.Andy.X.Storage.App.Extensions.DependencyInjection
{
    public static class IOServiceDependencyInjectionExtensions
    {
        public static void AddIOServices(this IServiceCollection services)
        {
            services.AddSingleton<SystemIOService>();
            services.AddSingleton<TenantIOService>();
            services.AddSingleton<ProducerIOService>();
            services.AddSingleton<ConsumerIOService>();
        }
    }
}
