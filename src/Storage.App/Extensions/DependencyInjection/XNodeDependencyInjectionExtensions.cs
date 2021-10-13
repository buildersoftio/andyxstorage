using Buildersoft.Andy.X.Storage.Core.Abstraction.Repository.Connection;
using Buildersoft.Andy.X.Storage.Core.Repository.Connection;
using Microsoft.Extensions.DependencyInjection;

namespace Buildersoft.Andy.X.Storage.App.Extensions.DependencyInjection
{
    public static class XNodeDependencyInjectionExtensions
    {
        public static void AddNodeServiceRepository(this IServiceCollection services)
        {
            services.AddSingleton<IXNodeConnectionRepository, XNodeConnectionRepository>();
        }
    }
}
