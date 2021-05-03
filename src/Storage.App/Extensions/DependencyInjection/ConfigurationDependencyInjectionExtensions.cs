using Buildersoft.Andy.X.Storage.Model.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace Buildersoft.Andy.X.Storage.App.Extensions.DependencyInjection
{
    public static class ConfigurationDependencyInjectionExtensions
    {
        public static void AddConfigurations(this IServiceCollection services, IConfiguration configuration)
        {
            services.BindXNodeConfiguration(configuration);
            services.BindDataStorageConfiguration(configuration);
            services.BindAgentConfiguration(configuration);
            services.BindPartitionConfiguration(configuration);
        }

        private static void BindXNodeConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var nodeConfiguration = new List<XNodeConfiguration>();
            configuration.Bind("XNodes", nodeConfiguration);
            services.AddSingleton(nodeConfiguration);
        }

        private static void BindDataStorageConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var storageConfiguration = new DataStorageConfiguration();
            configuration.Bind("DataStorage", storageConfiguration);
            services.AddSingleton(storageConfiguration);
        }

        private static void BindAgentConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var agentConfiguration = new AgentConfiguration();
            configuration.Bind("Agent", agentConfiguration);
            services.AddSingleton(agentConfiguration);
        }

        private static void BindPartitionConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var partitionConfiguration = new PartitionConfiguration();
            configuration.Bind("Partition", partitionConfiguration);
            services.AddSingleton(partitionConfiguration);
        }
    }
}
