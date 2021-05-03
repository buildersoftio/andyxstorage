using Microsoft.AspNetCore.Builder;
using Storage.Core.Service.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
