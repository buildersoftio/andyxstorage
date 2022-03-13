using Buildersoft.Andy.X.Storage.Core.Abstraction.Repository.Connection;
using Buildersoft.Andy.X.Storage.Core.Abstraction.Repository.Consumers;
using Buildersoft.Andy.X.Storage.Core.Repository.Connection;
using Buildersoft.Andy.X.Storage.Core.Repository.Consumers;
using Microsoft.Extensions.DependencyInjection;

namespace Buildersoft.Andy.X.Storage.App.Extensions.DependencyInjection
{
    public static class RepositoryInjectionExtensions
    {
        public static void AddNodeServiceRepository(this IServiceCollection services)
        {
            services.AddSingleton<IXNodeConnectionRepository, XNodeConnectionRepository>();
        }

        public static void AddConsumerConnectionRepository(this IServiceCollection services)
        {
            services.AddSingleton<IConsumerConnectionRepository, ConsumerConnectionRepository>();
        }
    }
}
