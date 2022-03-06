using Buildersoft.Andy.X.Storage.Core.Abstraction.Repository.Connection;
using Buildersoft.Andy.X.Storage.Core.Service.XNodes;
using Buildersoft.Andy.X.Storage.IO.Locations;
using Buildersoft.Andy.X.Storage.IO.Readers;
using Buildersoft.Andy.X.Storage.IO.Services;
using Buildersoft.Andy.X.Storage.IO.Writers;
using Buildersoft.Andy.X.Storage.Model.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace Buildersoft.Andy.X.Storage.Core.Service.System
{
    public class SystemService
    {
        private readonly ILogger<SystemService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IXNodeConnectionRepository _xNodeConnectionRepository;

        private readonly SystemIOService _systemIOService;
        private readonly TenantIOService _tenantIOService;
        private readonly ProducerIOService _producerIOService;
        private readonly ConsumerIOService _consumerIOService;
        private readonly MessageIOService _messageIOService;
        private readonly List<XNodeConfiguration> nodes;
        private readonly DataStorageConfiguration dataStorage;
        private readonly AgentConfiguration agent;
        private readonly PartitionConfiguration partition;
        private readonly CredentialsConfiguration credentials;


        public SystemService(
            ILogger<SystemService> logger,
            IServiceProvider serviceProvider,
            IXNodeConnectionRepository xNodeConnectionRepository,
            SystemIOService systemIOService,
            TenantIOService tenantIOService,
            ProducerIOService producerIOService,
            ConsumerIOService consumerIOService,
            MessageIOService messageIOService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

            _xNodeConnectionRepository = xNodeConnectionRepository;
            _systemIOService = systemIOService;
            _tenantIOService = tenantIOService;
            _producerIOService = producerIOService;
            _consumerIOService = consumerIOService;
            _messageIOService = messageIOService;
            nodes = _serviceProvider.GetService(typeof(List<XNodeConfiguration>)) as List<XNodeConfiguration>;
            dataStorage = _serviceProvider.GetService(typeof(DataStorageConfiguration)) as DataStorageConfiguration;
            agent = _serviceProvider.GetService(typeof(AgentConfiguration)) as AgentConfiguration;
            partition = _serviceProvider.GetService(typeof(PartitionConfiguration)) as PartitionConfiguration;
            credentials = _serviceProvider.GetService(typeof(CredentialsConfiguration)) as CredentialsConfiguration;


            DoFileConfiguration();

            UpdateXNodesConfiguration();
            UpdateDataStorageConfiguration();
            UpdateCredentials();

            InitializeServices();
        }

        private void UpdateXNodesConfiguration()
        {
            List<XNodeConfiguration> xNodes = nodes;
            if (File.Exists(SystemLocations.GetNodesConfigFile()))
            {
                xNodes = SystemConfigurationReader.ReadXNodesConfigurationFromFile();
                foreach (var node in nodes)
                {
                    var nodeExists = xNodes.Exists(x => x.ServiceUrl == node.ServiceUrl);
                    if (nodeExists != true)
                        xNodes.Add(node);
                }
            }
            SystemConfigurationWriter.WriteXNodesConfigurationFromFile(xNodes);
        }

        private void UpdateDataStorageConfiguration()
        {
            DataStorageConfiguration newConfig = dataStorage;
            if (File.Exists(SystemLocations.GetStorageCredentialsConfigFile()))
            {
                var actualConfig = SystemConfigurationReader.ReadStorageConfigurationFromFile();
                if (newConfig.Name != actualConfig.Name || newConfig.Status != actualConfig.Status)
                    SystemConfigurationWriter.WriteStorageConfigurationFromFile(newConfig);
            }
            else
                SystemConfigurationWriter.WriteStorageConfigurationFromFile(newConfig);
        }

        private void UpdateCredentials()
        {
            CredentialsConfiguration newConfig = credentials;
            if (File.Exists(SystemLocations.GetCredentialsConfigFile()))
            {
                var actualConfig = SystemConfigurationReader.ReadCredentialsConfigurationFromFile();
                if (newConfig.Username != actualConfig.Username || newConfig.Password != actualConfig.Password)
                    SystemConfigurationWriter.WriteCredentialsConfigurationFromFile(newConfig);
            }
            else
                SystemConfigurationWriter.WriteCredentialsConfigurationFromFile(newConfig);
        }

        private void DoFileConfiguration()
        {
            if (Directory.Exists(SystemLocations.GetConfigDirectory()) && Directory.Exists(SystemLocations.GetStorageDirectory()))
                return;

            _logger.LogInformation("Importing Configuration");
            _logger.LogWarning($"If configuration process failes make sure that user have access to write in this location {SystemLocations.GetRootDirectory()}");

            _systemIOService.CreateConfigDirectories();
        }

        private void InitializeServices()
        {
            _logger.LogInformation("Starting Services");
            _logger.LogInformation("Starting Agents");

            foreach (var xnode in nodes)
            {
                if (agent.LoadBalanced == false)
                {
                    for (int i = 0; i < agent.MaxNumber; i++)
                    {
                        string agentId = Guid.NewGuid().ToString();
                        _logger.LogInformation($"Agent '{agentId}' connection request sent");
                        var nodeEventsService = new XNodeEventService(
                            _logger,
                            agentId,
                            xnode,
                            dataStorage,
                            partition,
                            agent,
                            _xNodeConnectionRepository,
                            _tenantIOService,
                            _producerIOService,
                            _consumerIOService,
                            _messageIOService);
                    }
                }
                else
                {
                    _logger.LogError($"LoadBalanced property is 'true', this version of storage doesn't support it, please update it to 'false'");
                }
            }
        }
    }
}
