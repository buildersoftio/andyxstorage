using Buildersoft.Andy.X.Storage.Core.Service.System;
using Microsoft.AspNetCore.Builder;
using System;

namespace Buildersoft.Andy.X.Storage.App.Extensions.DependencyInjection
{
    public static class SystemServiceDependencyInjectionExtensions
    {
        public static void StartServices(this IApplicationBuilder builder, IServiceProvider provider)
        {
            var service = provider.GetService(typeof(SystemService));
        }
    }
}
